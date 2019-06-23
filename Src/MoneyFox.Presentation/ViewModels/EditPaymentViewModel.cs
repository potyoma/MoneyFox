﻿using System;
using System.Threading.Tasks;
using GalaSoft.MvvmLight.Views;
using GenericServices;
using MoneyFox.Foundation.Resources;
using MoneyFox.Presentation.Commands;
using MoneyFox.Presentation.Services;
using MoneyFox.ServiceLayer.Facades;
using MoneyFox.ServiceLayer.Utilities;
using IDialogService = MoneyFox.Presentation.Interfaces.IDialogService;

namespace MoneyFox.Presentation.ViewModels
{
    public class EditPaymentViewModel : ModifyPaymentViewModel
    {
        private readonly IPaymentService paymentService;
        private readonly INavigationService navigationService;
        private readonly IBackupService backupService;
        private readonly ICrudServices crudServices;
        private readonly IDialogService dialogService;

        public EditPaymentViewModel(IPaymentService paymentService,
            ICrudServices crudServices,
            IDialogService dialogService,
            ISettingsFacade settingsFacade, 
            IBackupService backupService,
            INavigationService navigationService) 
            : base(crudServices, dialogService, settingsFacade, backupService, navigationService)
        {
            this.paymentService = paymentService;
            this.navigationService = navigationService;
            this.crudServices = crudServices;
            this.dialogService = dialogService;
            this.backupService = backupService;
            this.paymentService = paymentService;
        }

        public int PaymentId { get; set; }

        /// <summary>
        ///     Delete the selected CategoryViewModel from the database
        /// </summary>
        public AsyncCommand DeleteCommand => new AsyncCommand(DeletePayment);

        public AsyncCommand InitializeCommand => new AsyncCommand(Initialize);

        protected override async Task Initialize()
        {
            SelectedPayment = crudServices.ReadSingle<PaymentViewModel>(PaymentId);

            // We have to set this here since otherwise the end date is null. This causes a crash on android.
            // Also it's user unfriendly if you the default end date is the 1.1.0001.
            if (SelectedPayment.IsRecurring && SelectedPayment.RecurringPayment.IsEndless)
            {
                SelectedPayment.RecurringPayment.EndDate = DateTime.Today;
            }

            Title = PaymentTypeHelper.GetViewTitleForType(SelectedPayment.Type, true);

            await base.Initialize();
        }

        protected override async Task SavePayment()
        {
            var result = await paymentService.UpdatePayment(SelectedPayment);

            if (!result.Success)
            {
                await dialogService.ShowMessage(Strings.GeneralErrorTitle, crudServices.GetAllErrors());
                return;
            }

            navigationService.GoBack();
        }

        private async Task DeletePayment()
        {
            await paymentService.DeletePayment(SelectedPayment);
#pragma warning disable 4014
            backupService.EnqueueBackupTask();
#pragma warning restore 4014
            CancelCommand.Execute(null);
        }
    }
}