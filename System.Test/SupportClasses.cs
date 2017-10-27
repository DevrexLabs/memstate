namespace System.Test
{
    using System;
    using System.Collections.Generic;

    using Memstate;

    public class AddStringCommand : Command<List<string>, int>
    {
        public String StringToAdd { get; set; }

        public override int Execute(List<string> model)
        {
            model.Add(StringToAdd);
            return model.Count;
        }
    }

    public class GetStringsQuery : Query<List<string>, List<string>>
    {
        public override List<string> Execute(List<string> model)
        {
            return model;
        }
    }
}
