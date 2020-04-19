using System;
using Memstate.Examples.Trello.Core;
using NUnit.Framework;

namespace Trello.Test
{
    public class TrelloTests
    {
        private TrelloModel _model;

        [SetUp]
        public void Setup()
        {
            _model = new TrelloModel();
        }

        [Test]
        public void CreateBoard()
        {
            var id = _model.CreateBoard("a board");
            Assert.AreEqual(8, id.Length);
        }

        [Test]
        public void GetBoardById()
        {
            var name = "a board";
            var id = _model.CreateBoard(name);
            var board = _model.Boards[id];
            Assert.AreEqual(name, board.Name);
        }

        [Test]
        public void GetBoards()
        {
            var name = "a board";
            var id = _model.CreateBoard(name);
            var board = _model.Boards[id];
            Assert.AreEqual(name, board.Name);
        }
    }
}