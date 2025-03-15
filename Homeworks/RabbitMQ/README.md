# Домашнее задание по RabbitMQ

Проект для домашних заданий и демо по курсу `C# ASP.NET Core Разработчик` от `Отус`.
Cистема `Promocode Factory` для выдачи промокодов партнеров для клиентов по группам предпочтений.

1) Реализовано с помощью библиотеки **MassTransit.RabbitMQ**

2) Приложение ReceivingFromPartner  - является отправителем

Создан exchange (обменник) c типом сообщений **IReceivePromoCodeFromPartnerMessage**

На этот тип сообщения подписаны **две очереди** - соответственно в приложении Administration и в приложении GivingToCustomer

3) Обработка полученных сообщений реализована через одинаковый сервис **PromoCodeService**.
   Сервиc расположен в проектах WebHost (а не в Core!).


![image](https://github.com/user-attachments/assets/f78e52ab-9191-42c5-9fa9-bde1be5c143b)


![image](https://github.com/user-attachments/assets/7a125837-00ae-45ce-8f8b-056b193100d4)
