namespace Functionator.Analyzer
{
    internal class Function
    {
        public Function(string name, string caller, FunctionType functionType)
        {
            Name = name;
            Caller = caller;
            FunctionType = functionType;
        }

        public string Name { get; set; }
        public string Caller { get; set; }
        public FunctionType FunctionType { get; set; }
        public bool IsOnTop { get; set; }
        public bool IsOnBottom { get; set; }
    }
}