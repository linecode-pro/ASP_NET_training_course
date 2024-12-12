using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using System.Threading.Tasks;
using System;
using Xunit;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.WebHost.Models;
using AutoFixture.AutoMoq;
using AutoFixture;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.Eventing.Reader;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private Mock<IRepository<Partner>> _partnersRepositoryMock;
        private PartnersController _partnersController;
        private SetPartnerPromoCodeLimitRequest _setPartnerPromoCodeLimitRequest;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _partnersRepositoryMock = fixture.Freeze<Mock<IRepository<Partner>>>();
            _partnersController = fixture.Build<PartnersController>().OmitAutoProperties().Create();
            _setPartnerPromoCodeLimitRequest = fixture.Build<SetPartnerPromoCodeLimitRequest>().Create();
        }

        // Фабрика партнеров - для применения фабричного метода при определении данных
        Partner CreateDefaultPartnersFixture(bool isActive = true, bool withPartnerLimits = true, bool withCancelDate = false, int num = 10)
        {
            var autoFixture = new Fixture();
            autoFixture.Customize<Partner>(x => x.With(par => par.PartnerLimits, new List<PartnerPromoCodeLimit>()));
            Partner partner = autoFixture.Create<Partner>();

            partner.IsActive = isActive;

            partner.NumberIssuedPromoCodes = num;

            if (withPartnerLimits)
            {
                if (withCancelDate)
                {
                    partner.PartnerLimits.Add(new PartnerPromoCodeLimit() {
                        Id = Guid.NewGuid(),
                        CreateDate = DateTime.Now,
                        EndDate = DateTime.Now.AddDays(14),
                        Limit = 50,
                        CancelDate = DateTime.Now
                    });
                }
                else
                {
                    partner.PartnerLimits.Add(new PartnerPromoCodeLimit() {
                        Id = Guid.NewGuid(),
                        CreateDate = DateTime.Now.AddDays(-14),
                        EndDate = DateTime.Now.AddDays(2),
                        Limit = 50
                    });
                }
            }
            
            return partner;
        }



        // 1. Если партнер не найден, то также нужно выдать ошибку 404
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotFound_ReturnsNotFound()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            Partner partner = null;
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partnerId)).ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, _setPartnerPromoCodeLimitRequest);

            // Assert
            result.Should().BeAssignableTo<NotFoundResult>();
        }




        // 2. Если партнер заблокирован, то есть поле IsActive=false в классе Partner, то также нужно выдать ошибку 400
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_PartnerIsNotActive_ReturnsBadRequest()
        {
            // Arrange                       
            Partner partner = CreateDefaultPartnersFixture(false); // фабричный метод
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _setPartnerPromoCodeLimitRequest);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }




        // 3. Если партнеру выставляется лимит, то мы должны обнулить количество промокодов,
        // которые партнер выдал NumberIssuedPromoCodes, если лимит закончился, то количество не обнуляется;
        // В нашем случае - создадим три метода проверки:
        // 1 и 2 - это разбить нашу проверку на два метода [Fact]
        // 3 - это один метод [Theory], который сам разбивается на два метода и имеет два параметра на входе

        // 3.1
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_WithLimit_NumberIssuedPromoCodesIsZero()
        {
            // Arrange
            Partner partner = CreateDefaultPartnersFixture(); // фабричный метод            
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _setPartnerPromoCodeLimitRequest);

            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(0);
        }

        
        // 3.2
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_WithoutLimit_NumberIssuedPromoCodesIsNotZero()
        {
            // Arrange
            int num = 33;
            Partner partner = CreateDefaultPartnersFixture(true, true, true, num); // фабричный метод
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _setPartnerPromoCodeLimitRequest);

            // Assert
            partner.NumberIssuedPromoCodes.Should().Be(num);
        }


        // 3.3
        public static TheoryData<bool> WithCancelDates = new()
                                                    {
                                                        { true },
                                                        { false }
                                                    };

        [Theory]
        [MemberData(nameof(WithCancelDates))]
        public async Task SetPartnerPromoCodeLimitAsync_WithoutLimitOrWithLimit_NumberIssuedPromoCodesIsNotZeroOrZero(bool withCancelDate)
        {
            // Arrange
            int num = 33;
            Partner partner = CreateDefaultPartnersFixture(true, true, withCancelDate, num); // фабричный метод
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _setPartnerPromoCodeLimitRequest);

            // Assert
            if (withCancelDate)
            {
                partner.NumberIssuedPromoCodes.Should().Be(num);
            }
            else
            {
                partner.NumberIssuedPromoCodes.Should().Be(0);
            }           
        }




        // 4. При установке лимита нужно отключить предыдущий лимит
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsync_WithLimit_CancelDateNotBeNull()
        {
            // Arrange
            Partner partner = CreateDefaultPartnersFixture();
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _setPartnerPromoCodeLimitRequest);

            // Assert
            partner.PartnerLimits.ElementAt(0).CancelDate.Should().NotBe(null);
        }



        // 5. Лимит должен быть больше 0
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task SetPartnerPromoCodeLimitAsync_LimitIsNotGreaterThan0_ReturnsBadRequest(int limit)
        {
            // Arrange
            var partner = CreateDefaultPartnersFixture();
            _setPartnerPromoCodeLimitRequest.Limit = limit;
            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, _setPartnerPromoCodeLimitRequest);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }



        // 6. Нужно убедиться, что сохранили новый лимит в базу данных
        [Fact]
        public async Task SetPartnerPromoCodeLimitAsyncTests_SaveToDataBase_SuccessUpdate()
        {
            // Arrange
            var partner = CreateDefaultPartnersFixture();
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();

            _partnersRepositoryMock.Setup(repo => repo.GetByIdAsync(partner.Id)).ReturnsAsync(partner);

            // Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            // Assert
            _partnersRepositoryMock.Verify(repo => repo.UpdateAsync(partner), Times.Once);
        }
    }
}