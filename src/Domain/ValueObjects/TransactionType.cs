namespace Domain.ValueObjects;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer,
    OverdraftFee,
    InterestAccrual,
    ServiceCharge,
    Reversal,
    Adjustment,
}
