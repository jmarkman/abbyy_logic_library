namespace WKFCBusinessRules
{
    public class AcordAddress
    {
        public string Insured { get; set; }
        public string[] Address { get; set; }

        public AcordAddress() { }

        public AcordAddress(string insuredName, string[] addressParts)
        {
            Insured = insuredName;
            Address = addressParts;
        }
    }
}
