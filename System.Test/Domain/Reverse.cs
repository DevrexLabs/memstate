namespace System.Test
{
    using System.Collections.Generic;

    using Memstate;

    public partial class SystemTests
    {
        public class Reverse : Command<List<string>>
        {
            public override void Execute(List<string> model)
            {
                model.Reverse();
            }
        }
    }
}