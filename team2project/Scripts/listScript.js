﻿(function ($) {
    $.widget("custom.combobox", {
        _create: function () {
            this.wrapper = $("<span>")
              .addClass("custom-combobox")
              .insertAfter(this.element);

            this.element.hide();
            this._createAutocomplete();
            this._createShowAllButton();
        },

        _createAutocomplete: function () {
            var selected = this.element.children(":selected"),
              value = selected.val() ? selected.text() : "";

            this.input = $("<input>")
              .appendTo(this.wrapper)
              .val(value)
              .attr("title", "")
              .addClass("custom-combobox-input ui-widget ui-widget-content ui-state-default ui-corner-left")
              .autocomplete({
                  delay: 0,
                  minLength: 0,
                  source: $.proxy(this, "_source")
              })
              .tooltip({
                  tooltipClass: "ui-state-highlight"
              });

            this._on(this.input, {
                autocompleteselect: function (event, ui) {
                    ui.item.option.selected = true;
                    this._trigger("select", event, {
                        item: ui.item.option
                    });
                    
                    var url = location.protocol + '//' + location.host + '/', path = location.pathname.split('/');
                    url += path[1];
                    if (ui.item.value && ui.item.option.value !== '-1') {
                        url += '/' + ui.item.option.value;
                    }
                    if (isSecondDays) url += '/' + path[2];
                    else if (path[3]) url += '/' + path[3];
                    document.location.href = url;
                    setTimeout(function () {
                        document.location.href = url;
                    }, 1000);
                },

                autocompletechange: "_removeIfInvalid"
            });
        },

        _createShowAllButton: function () {
            var input = this.input,
              wasOpen = false;

            $("<a>")
              .attr("tabIndex", -1)
              .attr("title", "Показать все города")
              .tooltip()
              .appendTo(this.wrapper)
              .button({
                  icons: {
                      primary: "ui-icon-triangle-1-s"
                  },
                  text: false
              })
              .removeClass("ui-corner-all")
              .addClass("custom-combobox-toggle ui-corner-right")
              .mousedown(function () {
                  wasOpen = input.autocomplete("widget").is(":visible");
              })
              .click(function () {
                  input.focus();

                  // Close if already visible
                  if (wasOpen) {
                      return;
                  }

                  // Pass empty string as value to search for, displaying all results
                  input.autocomplete("search", "");
              });
        },

        _source: function (request, response) {
            var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
            response(this.element.children("option").map(function () {
                var text = $(this).text();
                if (this.value && (!request.term || matcher.test(text)))
                    return {
                        label: text,
                        value: text,
                        option: this
                    };
            }));
        },

        _removeIfInvalid: function (event, ui) {

            // Selected an item, nothing to do
            if (ui.item) {
                return;
            }

            // Search for a match (case-insensitive)
            var value = this.input.val(),
              valueLowerCase = value.toLowerCase(),
              valid = false;
            this.element.children("option").each(function () {
                if ($(this).text().toLowerCase() === valueLowerCase) {
                    this.selected = valid = true;
                    return false;
                }
            });

            // Found a match, nothing to do
            if (valid) {
                return;
            }

            // Remove invalid value
            this.input
              .val("")
              .attr("title", value + " didn't match any item")
              .tooltip("open");
            this.element.val("");
            this._delay(function () {
                this.input.tooltip("close").attr("title", "");
            }, 2500);
            this.input.autocomplete("instance").term = "";
        },

        _destroy: function () {
            this.wrapper.remove();
            this.element.show();
        }
    });
})(jQuery);

$(function () {
    var combobox = $("#combobox"), date = $("#datefilter"),
        path = location.pathname.split('/');

    if (isSecondDays) date.find('option[value="' + (path[2]) + '"]').attr('selected', 'selected');
    else date.find('option[value="' + (path[3] ? path[3] : '') + '"]').attr('selected', 'selected');
    combobox.find('option[value="' + (path[2] ? path[2] : '') + '"]').attr('selected', 'selected');

    combobox.combobox();
    $("#toggle").click(function () {
        combobox.toggle();
    });
    date.change(function () {
        var url = location.protocol + '//' + location.host + '/';
        url += path[1];
        var locat = path[2];
        if (!isSecondDays && locat) url += '/' + locat;
        if (date.val() && date.val()!=='-1') {
            url += '/' + date.val();
        }
        document.location.href = url;
        setTimeout(function () {
            document.location.href = url;
        }, 1000);
    });
});