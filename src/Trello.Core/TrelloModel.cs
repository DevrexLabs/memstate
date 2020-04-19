using System;
using System.Collections.Generic;

namespace Memstate.Examples.Trello.Core
{
    public class TrelloModel
    {
        private Random _idGenerator = new Random(42);

        public string NextId()
        {
            var buf = new char[8];
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            for (int i = 0; i < buf.Length; i++)
            {
                var index = _idGenerator.Next(chars.Length);
                buf[i] = chars[index];
            }
            return new string(buf);
        }
        public Dictionary<string, Board> Boards { get; set; }

        public TrelloModel()
        {
            Boards = new Dictionary<string, Board>();
        }

        public string CreateBoard(string name)
        {
            var id = NextId();
            var board = new Board(id, name);
            Boards.Add(board.Id, board);
            return board.Id;
        }
    }
}