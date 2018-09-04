namespace System.Test
{
    using System.Collections.Generic;

    using Memstate;

    public class AddStringCommand : Command<List<string>, int>
    {
        public AddStringCommand(string stringToAdd)
        {
            Ensure.NotNull(stringToAdd, nameof(stringToAdd));
            StringToAdd = stringToAdd;
        }

        public string StringToAdd { get; set; }

        public override int Execute(List<string> model)
        {
            model.Add(StringToAdd);
            return model.Count;
        }
    }
}