(function ($) {
    'use strict';
    $.fn.getCursorPosition = function () {
        var input = this.get(0);
        if (!input) return; // No (input) element found
        if ('selectionStart' in input) {
            // Standard-compliant browsers
            return input.selectionStart;
        } else if (document.selection) {
            // IE
            input.focus();
            var sel = document.selection.createRange();
            var selLen = document.selection.createRange().text.length;
            sel.moveStart('character', -input.value.length);
            return sel.text.length - selLen;
        }
    }
    $.fn.onlyNumbers = function(options){
        if(options == "destroy"){
            $(this).unbind("keypress");
        }
        else{
            var wholeNumberRegExp = new RegExp(/^\d+$/);
            var decimalRegExp = new RegExp(/^\d+(?:\.\d{1,2})?$/);
            var settings = $.extend({
                decimal: true,
                negative: true
            }, options);
            var self = this;
            self.keypress(function(e){
                var code = e.keyCode || e.which;
                var currVal = $(this).val();
                var character = String.fromCharCode(code);
                if(($.isEmptyObject(currVal) || 
                    ($(this).getCursorPosition() === 0 && currVal.indexOf("-") !== 0)) && character === "-" && settings.negative)
                {}
                else{
                    if(settings.negative && (currVal + character).indexOf("-") > 0){
                        e.preventDefault();
                    }
                    else{
                        if(!settings.decimal && !wholeNumberRegExp.test(character))
                            e.preventDefault();
                        else if (settings.decimal && code != 46 && code > 31 && (code < 48 || code > 57) || 
                                 ((currVal.replace("-", "") + character).split(".").length -1) > 1)
                            e.preventDefault();
                    }
                }
            });
        }
        return this;
    };
    var timeout;
    $.fn.leLoader = function (options) {
        if (options == 'stop') {
            clearTimeout(timeout);
            combineDots();
            setTimeout(function () {
                $(".dot-loader").animate({ top: "-100" }, 200, function () {
                    $(this).remove();
                    $("#dot-overlay").remove();
                });
            }, 1200);
        }
        else {
            var settings = $.extend({
                overlayClass: "overlay"
            }, options);
            this.append("<div id='dot-overlay' class='" + settings.overlayClass + "'></div>");
            this.append('<div class="dot-loader" data-run="started"><span class="dot first">' +
                '</span><span class="dot second"></span><span class="dot third"></span></div>');
            $("#dot-overlay").fadeIn(200, function () {
                continousRun(true);
            });
        }
        return this;

    }
    function continousRun(run) {
        if (run) {
            timeout = setTimeout(function () {
                roatateDots($(".dot-loader .dot.first"));
                roatateDots($(".dot-loader .dot.second"));
                roatateDots($(".dot-loader .dot.third"));
                continousRun($(".dot-loader").data("run") != 'stop');
            }, 600);
        }
        else {
            combineDots();
        }
    }
    function roatateDots(elm) {
        var top = elm.css("top").replace("px", "");
        var left = elm.css("left").replace("px", "");
        if (top == 0 && left == 0)
            elm.animate({ top: 35 }, 500, "easeInOutBack");
        else if (top == 35 && left == 0)
            elm.animate({ left: 35 }, 500, "easeInOutBack");
        else if (top == 35 && left == 35)
            elm.animate({ top: 0 }, 500, "easeInOutBack");
        else
            elm.animate({ left: 0 }, 500, "easeInOutBack");
    }
    function combineDots() {
        $(".dot-loader .dot").animate({ top: "50%", left: "50%", marginTop: "-7.5px", marginLeft: "-7.5px" }, function () {
            $(this).addClass("done")
        });
    }
})(jQuery);