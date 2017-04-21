﻿using Windows.ApplicationModel.Background;
using MoneyFox.Business.Manager;
using MoneyFox.DataAccess;
using MoneyFox.DataAccess.Infrastructure;
using MoneyFox.DataAccess.Repositories;
using MoneyFox.Service.DataServices;

namespace MoneyFox.Windows.Tasks
{
    public sealed class RecurringPaymentTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            try
            {
                MapperConfiguration.Setup();

                var dbFactory = new DbFactory();
                var paymentRepository = new PaymentRepository(dbFactory);

                new RecurringPaymentManager(new RecurringPaymentService(new RecurringPaymentRepository(dbFactory)),
                    new PaymentService(new UnitOfWork(dbFactory), new PaymentRepository(dbFactory)))
                    .CreatePaymentsUpToRecur();
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}