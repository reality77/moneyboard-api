namespace dto
{
    public enum ECurrency
    {
        Unknown = 0,

        EUR = 1,
    }

    public enum ETransactionType : int
    {
        Unknown = 0,
        
        // Paiment
        Payment = 1,

        //Virement bancaire
        Transfer = 2,

        // Retrait d'espèces
        Withdrawal = 3,

        // Prélèvement bancaire
        Debit = 4,

        // Frais bancaire
        Fees = 5
    }

    public enum ERecognitionRuleConditionFieldType : int
    {
        DataField = 0,
        Tag = 1
    }

    public enum ERecognitionRuleConditionOperator : int
    {
        Equals = 0,
        Contains = 1,
        MatchRegex = 2,
        Greater = 10,
        GreaterThan = 11,
        Lower = 12,
        LowerThan = 13,
    }

    public enum ERecognitionRuleActionType : int
    {
        SetData = 0,
        AddTag = 1,
    }
}