// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function Trello(ko) {
    let trello = {
        CardViewModel : function({id,title,body}) {
            this.id = id;
            this.title = title;
            this.body = body;
        }
    };
    
    trello.ColumnViewModel = function({id, name}) {
        let self = this;
        self.id = id;
        self.name = ko.observable(name);
        self.cards = ko.observableArray();
        self.addCard = function(card) {
            self.cards.push(new trello.CardViewModel(card));
        };

        self.loadCards = function() {
            $.getJSON('/Trello/Cards/' + self.id, function(cards) {
                for(let card of cards) {
                    self.addCard(card);
                }
            });
        }
    };
    
    trello.BoardViewModel = function(id, name) {
        let self = this;
        self.id = id;
        self.name = ko.observable(name);
        self.columns = ko.observableArray();
        self.loadColumns = function() {
            $.getJSON('/Trello/Columns/' + self.id, function(columns) {
                for(let col of columns) {
                    let colViewModel = new trello.ColumnViewModel(col);
                    self.columns.push(colViewModel);
                    colViewModel.loadCards();
                }
            });
        };
        self.addColumn = function({id, name}){
            self.columns.push(new trello.ColumnViewModel({id,name}));
        }
    };
    return trello;
}
