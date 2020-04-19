using Memstate.Examples.Trello.Core;
using NUnit.Framework;

namespace Trello.Test
{
    public class QueryTests
    {
        private TrelloModel _model;

        [SetUp]
        public void Setup()
        {
            _model = new TrelloModel();
        }

        [Test]
        public void GetBoards()
        {
            _model.CreateBoard("c");
            _model.CreateBoard("a");
            _model.CreateBoard("B");

            var query = new GetBoardsQuery();
            var boards = query.Execute(_model);
            Assert.AreEqual(3, boards.Count);
            Assert.AreEqual("a", boards[0].Name);
            Assert.AreEqual("B", boards[1].Name);
            Assert.AreEqual("c", boards[2].Name);
        }
    }
}