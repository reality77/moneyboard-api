namespace dal.Model
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

}