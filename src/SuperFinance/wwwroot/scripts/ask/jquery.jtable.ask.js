/*
* Copyright (C) Khosla Tech Private Limited. All rights reserved.
* Part of ASP Security Kit (https://ASPSecurityKit.net )
* Author: Varun Om Khosla
* This plugin extends jQuery.jtable.js with useful features such as:
* proper naming of add/edit buttons based on either modelName (custom options property) or title (singularizes first).
* New getRecords function
* Ability to add custom buttons on rows just as toolbar buttons with options.rowButtons collection.
* Ability to specify options.AntiForgeryToken which is then automatically added to every ajax call.
*/

(function ($) {
    var plugin = $.hik.jtable.prototype;
    // store copies of the original plugin functions before overwriting
    var base = {};
    for (var i in plugin) {
        if (typeof (plugin[i]) === 'function') {
            base[i] = plugin[i];
        }
    }

    $.extend(true, plugin, {
        _create: function () {
            var title = this.options.title;
            var modelName = this.options.modelName;
            var theType = null;
            if (modelName && modelName.length > 0) {
                theType = modelName;
            }  else if (title && title.length > 0) {
                theType = title;
                var pairs = [{ k: 'ies', v: 'y' }, { k: 's', v: '' }];
                for (var i = 0; i < pairs.length; i++) {
                    var p = pairs[i];
                    if (title.length > p.k.length && title.substr(title.length - p.k.length).toLowerCase() == p.k.toLowerCase()) {
                        theType = title.substr(0, title.length - p.k.length) + p.v;
                        break;
                    }
                }
            }

            if (theType){
                this.options.messages.editRecord = 'Edit ' + theType;
                this.options.messages.addNewRecord = 'New ' + theType;
            }
            
            base._create.apply(this, arguments);
        },

        /*
        * Extends jtable to change options post initialization. Note: design to work with static settings like actions url etc. does not work as intended for options like toolbar/fields as those are processed during initialization.
* Make appropriate changes to extend it further.
        */
        _setOption: function (key, value) {
            if (typeof this.options[key] !== "object")
                this.options[key] = value;
            else
                $.extend(true, this.options[key], value);
            base._setOption.apply(this, arguments);
        },

        /*
        * Gets all the records currently bound.
*/
        getRecords: function () {
            var records = [];
            for (var i = 0; i < this._$tableRows.length; i++) {
                records.push(this._$tableRows[i].data('record'));
            }
            return records;
        },

        /*
        * extends jtable to add column cell(s) to header row for custom buttons.
*/
        _addColumnsToHeaderRow: function ($tr) {
            base._addColumnsToHeaderRow.apply(this, arguments);
            if (this.options.rowButtons != undefined) {
                for (var i = 0; i < this.options.rowButtons.length; i++)
                    $tr.append(this._createEmptyCommandHeader());
            }
        },

        /*
* extends jtable to add custom button(s) to a row.
*/
        _addCellsToRowUsingRecord: function ($row) {
            if (this.options.editOnRowClick) {
                this.options.selectOnRowClick = false;
            }

            var self = this;
            base._addCellsToRowUsingRecord.apply(this, arguments);

            if (this.options.rowButtons != undefined) {
                $row.customButtons = [];
                for (var i = 0; i < this.options.rowButtons.length; i++) {
                    $row.customButtons.push(this._addCustomActionButtonToRow($row, this.options.rowButtons[i](
                        {
                            record: $row.data('record')
                        })));
                }
            }

            if (self.options.actions.updateAction != undefined && self.options.editOnRowClick) {
                $row.click(function () {
                    self._showEditForm($row);
                });
            }

            if (self.options.rowClicked) {
                $row.click(function () {
                    self.options.rowClicked({
                        record: $row.data('record')
                    });
                });
            }
        },

        _addCustomActionButtonToRow: function ($row, button) {
            var customButton = { callback: button.click };
            var $span = $('<span></span>').html(button.text);
            var $button = $('<button title="' + button.text + '"></button>')
                .addClass('jtable-command-button ' + button.cssClass)
                .append($span)
                .click(function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    customButton.callback({ record: $row.data('record') });
                });
                        $('<td></td>')
                .addClass('jtable-command-column')
                .append($button)
                .appendTo($row);

                        customButton.$button = $button;
                        customButton.$span = $span;
                        return customButton;
        },

        _updateRowTexts: function ($tableRow) {
            base._updateRowTexts.apply(this, arguments);

            if (this.options.rowButtons != undefined) {
                for (var i = 0; i < this.options.rowButtons.length; i++) {
                    var customButton = $tableRow.customButtons[i];
                     var button = this.options.rowButtons[i]({
                            record: $tableRow.data('record')
                        });

                     customButton.$span.html(button.text);
                     customButton.$button.attr("class", 'jtable-command-button ' + button.cssClass)
                    .attr("title", button.text);
                     customButton.callback = button.click;
                }
            }
        },

        _ajax: function (options) {
            if (this.options.AntiForgeryToken) {
                if (typeof options.data === 'string'){
                    options.data+= (options.data ? "&" : "") + $.param({"__RequestVerificationToken": this.options.AntiForgeryToken});
                    }
                    else if (typeof options.data === 'object' ){
                    options.data["__RequestVerificationToken"] = this.options.AntiForgeryToken;
                }
            }
            
            base._ajax.apply(this, arguments);
        }
    });

})(jQuery);
