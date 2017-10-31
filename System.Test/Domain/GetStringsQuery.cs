namespace System.Test
{
    using System.Collections.Generic;

    using Memstate;

    public class GetStringsQuery : Query<List<string>, List<string>>
    {
        public override List<string> Execute(List<string> model)
        {
            return model;
        }
    }
}
