namespace AdaptiveEMA
{
    public class RunParameters
    {
        private const double MinDefaultScaleConst = 0;
        private const double MaxDefaultScaleConst = 1;
        private const int PolyOrderDefault = 2;
    
        public RunParameters(            
            int windowSize, 
            int polyOrder = PolyOrderDefault, 
            double minScale = MinDefaultScaleConst, 
            double maxScale = MaxDefaultScaleConst,
            bool swallowValidation = false)
        {
            WindowSize = windowSize;
            PolyOrder = polyOrder;
            MinScale = minScale;
            MaxScale = maxScale;
            SwallowValidation = swallowValidation;
        }
        
        public RunParameters(int windowSize)
        {
            WindowSize = windowSize;
            PolyOrder = PolyOrderDefault;
            MinScale = MinDefaultScaleConst;
            MaxScale = MaxDefaultScaleConst;
            
            SwallowValidation = false;
        }
    
        public int WindowSize { get; } 
        public int PolyOrder { get; private set; } 
        public double MinScale { get; private set; } 
        public double MaxScale { get; private set; }
        internal bool SwallowValidation { get; private set; }
    }
}