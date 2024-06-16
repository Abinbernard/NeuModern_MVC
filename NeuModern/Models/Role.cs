namespace NeuModern.Models
{
    public class Role
    {
        public const string Role_Customer = "Customer";
        public const string Role_Admin = "Admin";



        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";


        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
        public const string PaymentStatusRejected = "Rejected";
        public const string PaymentStatusRefunded = "Refunded";



        public const string PaymentMethodOnline = "OnlinePayment";
        public const string PaymentMethodCOD = "COD";
        public const string PaymentMethodCODPending = "CODPending";
        public const string PaymentMethodWallet = "Wallet";

        public const string CouponValid = "valid";
        public const string CouponInValid = "Invalid";

        public const string IsValid = "valid";
        public const string IsInValid = "Invalid";


    }
}
