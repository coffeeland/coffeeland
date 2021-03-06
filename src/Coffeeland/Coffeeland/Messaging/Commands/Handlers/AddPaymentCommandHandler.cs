﻿using System;
using Coffeeland.Database;
using Coffeeland.Messaging.Commands.Commands;
using Coffeeland.Messaging.Dtos;
using Coffeeland.Messaging.Shared;
using Coffeeland.Session;
using System.Threading;
using Coffeeland.MailService;
using Coffeeland.Payments;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Coffeeland.Messaging.Commands.Handlers
{
    public class AddPaymentCommandHandler : ICommandHandler<AddPaymentCommand>
    {
        public IResult Handle(AddPaymentCommand command)
        {
            int clientId = SessionRepository.GetClientIdOfSession(command.sessionToken);
            if (clientId == -1)
                throw new Exception();

            var order = DatabaseQueryProcessor.GetTheMostRecentOrder(clientId);
            if (order == null)
            {
                throw new Exception();
            }

            var totalPrice = DatabaseQueryProcessor.GetTotal(order.orderId);

            DatabaseQueryProcessor.CreateNewPayment(
                        command.paymentId,
                        order.orderId,
                        totalPrice,
                        DateTime.Now.ToString("yyyy-MM-dd")
                    );

            ThreadPool.QueueUserWorkItem(
                o => new OrderPlacementEmail().Send(clientId));


            
           var isSuccessPayment = PaymentMethod.Check(command.paymentId, totalPrice);
           if (isSuccessPayment)
           {
                DatabaseQueryProcessor.UpdateOrder(order.orderId, 1);
                ThreadPool.QueueUserWorkItem( o =>
                    new SuccessfullPaymentEmail().Send(clientId));
           }
           else
           {
                ThreadPool.QueueUserWorkItem(o =>
                    new UnsuccessfullPaymentEmail().Send(clientId));
           }
            
            return new SuccessInfoDto
            {
                isSuccess = true
            };
        }
    }
}