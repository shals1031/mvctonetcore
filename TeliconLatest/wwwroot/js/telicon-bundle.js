/*
 * jQuery idleTimer plugin
 * version 0.8.092209
 * by Paul Irish. 
 *   http://github.com/paulirish/yui-misc/tree/
 * MIT license
 
 * adapted from YUI idle timer by nzakas:
 *   http://github.com/nzakas/yui-misc/
 
 
 * Copyright (c) 2009 Nicholas C. Zakas
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

(function($){

$.idleTimer = function f(newTimeout){

    //$.idleTimer.tId = -1     //timeout ID

    var idle    = false,        //indicates if the user is idle
        enabled = true,        //indicates if the idle timer is enabled
        timeout = 30000,        //the amount of time (ms) before the user is considered idle
        events  = 'mousemove keydown DOMMouseScroll mousewheel mousedown', // activity is one of these events
      //f.olddate = undefined, // olddate used for getElapsedTime. stored on the function
        
    /* (intentionally not documented)
     * Toggles the idle state and fires an appropriate event.
     * @return {void}
     */
    toggleIdleState = function(){
    
        //toggle the state
        idle = !idle;
        
        // reset timeout counter
        f.olddate = +new Date;
        
        //fire appropriate event
        $(document).trigger(  $.data(document,'idleTimer', idle ? "idle" : "active" )  + '.idleTimer');            
    },

    /**
     * Stops the idle timer. This removes appropriate event handlers
     * and cancels any pending timeouts.
     * @return {void}
     * @method stop
     * @static
     */         
    stop = function(){
    
        //set to disabled
        enabled = false;
        
        //clear any pending timeouts
        clearTimeout($.idleTimer.tId);
        
        //detach the event handlers
        $(document).unbind('.idleTimer');
    },
    
    
    /* (intentionally not documented)
     * Handles a user event indicating that the user isn't idle.
     * @param {Event} event A DOM2-normalized event object.
     * @return {void}
     */
    handleUserEvent = function(){
    
        //clear any existing timeout
        clearTimeout($.idleTimer.tId);
        
        
        
        //if the idle timer is enabled
        if (enabled){
        
          
            //if it's idle, that means the user is no longer idle
            if (idle){
                toggleIdleState();           
            } 
        
            //set a new timeout
            $.idleTimer.tId = setTimeout(toggleIdleState, timeout);
            
        }    
     };
    
      
    /**
     * Starts the idle timer. This adds appropriate event handlers
     * and starts the first timeout.
     * @param {int} newTimeout (Optional) A new value for the timeout period in ms.
     * @return {void}
     * @method $.idleTimer
     * @static
     */ 
    
    
    f.olddate = f.olddate || +new Date;
    
    //assign a new timeout if necessary
    if (typeof newTimeout == "number"){
        timeout = newTimeout;
    } else if (newTimeout === 'destroy') {
        stop();
        return this;  
    } else if (newTimeout === 'getElapsedTime'){
        return (+new Date) - f.olddate;
    }
    
    //assign appropriate event handlers
    $(document).bind($.trim((events+' ').split(' ').join('.idleTimer ')),handleUserEvent);
    
    
    //set a timeout to toggle state
    $.idleTimer.tId = setTimeout(toggleIdleState, timeout);
    
    // assume the user is active for the first x seconds.
    $.data(document,'idleTimer',"active");
      
    

    
}; // end of $.idleTimer()

    

})(jQuery);

/*
 * jQuery Idle Timeout 1.2
 * Copyright (c) 2011 Eric Hynds
 *
 * http://www.erichynds.com/jquery/a-new-and-improved-jquery-idle-timeout-plugin/
 *
 * Depends:
 *  - jQuery 1.4.2+
 *  - jQuery Idle Timer (by Paul Irish, http://paulirish.com/2009/jquery-idletimer-plugin/)
 *
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 *
*/

(function($, win){
	
	var idleTimeout = {
		init: function( element, resume, options ){
			var self = this, elem;

			this.warning = elem = $(element);
			this.resume = $(resume);
			this.options = options;
			this.countdownOpen = false;
			this.failedRequests = options.failedRequests;
			this._startTimer();
      		this.title = document.title;
			
			// expose obj to data cache so peeps can call internal methods
			$.data( elem[0], 'idletimeout', this );
			
			// start the idle timer
			$.idleTimer(options.idleAfter * 1000);
			
			// once the user becomes idle
			$(document).bind("idle.idleTimer", function(){
				
				// if the user is idle and a countdown isn't already running
				if( $.data(document, 'idleTimer') === 'idle' && !self.countdownOpen ){
					self._stopTimer();
					self.countdownOpen = true;
					self._idle();
				}
			});
			
			// bind continue link
			this.resume.bind("click", function(e){
				e.preventDefault();
				
				win.clearInterval(self.countdown); // stop the countdown
				self.countdownOpen = false; // stop countdown
				self._startTimer(); // start up the timer again
				self._keepAlive( false ); // ping server
				options.onResume.call( self.warning ); // call the resume callback
			});
		},
		
		_idle: function(){
			var self = this,
				options = this.options,
				warning = this.warning[0],
				counter = options.warningLength;
				
			// fire the onIdle function
			options.onIdle.call(warning);
			
			// set inital value in the countdown placeholder
			options.onCountdown.call(warning, counter);
			
			// create a timer that runs every second
			this.countdown = win.setInterval(function(){
				if(--counter === 0){
					window.clearInterval(self.countdown);
					options.onTimeout.call(warning);
				} else {
					options.onCountdown.call(warning, counter);
          document.title = options.titleMessage.replace('%s', counter) + self.title;
				}
			}, 1000);
		},
		
		_startTimer: function(){
			var self = this;

			this.timer = win.setTimeout(function(){
				self._keepAlive();
			}, this.options.pollingInterval * 1000);
		},
		
		_stopTimer: function(){
			// reset the failed requests counter
			this.failedRequests = this.options.failedRequests;
			win.clearTimeout(this.timer);
		},
		
		_keepAlive: function( recurse ){
			var self = this,
				options = this.options;
				
			//Reset the title to what it was.
			document.title = self.title;
			
			// assume a startTimer/keepAlive loop unless told otherwise
			if( typeof recurse === "undefined" ){
				recurse = true;
			}
			
			// if too many requests failed, abort
			if( !this.failedRequests ){
				this._stopTimer();
				options.onAbort.call( this.warning[0] );
				return;
			}
			
			$.ajax({
				timeout: options.AJAXTimeout,
				url: options.keepAliveURL,
				error: function(){
					self.failedRequests--;
				},
				success: function(response){
					if($.trim(response) !== options.serverResponseEquals){
						self.failedRequests--;
					}
				},
				complete: function(){
					if( recurse ){
						self._startTimer();
					}
				}
			});
		}
	};
	
	// expose
	$.idleTimeout = function(element, resume, options){
		idleTimeout.init( element, resume, $.extend($.idleTimeout.options, options) );
		return this;
	};
	
	// options
	$.idleTimeout.options = {
		// number of seconds after user is idle to show the warning
		warningLength: 30,
		
		// url to call to keep the session alive while the user is active
		keepAliveURL: "",
		
		// the response from keepAliveURL must equal this text:
		serverResponseEquals: "OK",
		
		// user is considered idle after this many seconds.  10 minutes default
		idleAfter: 600,
		
		// a polling request will be sent to the server every X seconds
		pollingInterval: 60,
		
		// number of failed polling requests until we abort this script
		failedRequests: 5,
		
		// the $.ajax timeout in MILLISECONDS!
		AJAXTimeout: 250,
		
		// %s will be replaced by the counter value
    	titleMessage: 'Warning: %s seconds until log out | ',
		
		/*
			Callbacks
			"this" refers to the element found by the first selector passed to $.idleTimeout.
		*/
		// callback to fire when the session times out
		onTimeout: $.noop,
		
		// fires when the user becomes idle
		onIdle: $.noop,
		
		// fires during each second of warningLength
		onCountdown: $.noop,
		
		// fires when the user resumes the session
		onResume: $.noop,
		
		// callback to fire when the script is aborted due to too many failed requests
		onAbort: $.noop
	};
	
})(jQuery, window);
/* =========================================================
 * bootstrap-datepicker.js
 * Repo: https://github.com/eternicode/bootstrap-datepicker/
 * Demo: http://eternicode.github.io/bootstrap-datepicker/
 * Docs: http://bootstrap-datepicker.readthedocs.org/
 * Forked from http://www.eyecon.ro/bootstrap-datepicker
 * =========================================================
 * Started by Stefan Petre; improvements by Andrew Rowls + contributors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * ========================================================= */

(function($, undefined){

	var $window = $(window);

	function UTCDate(){
		return new Date(Date.UTC.apply(Date, arguments));
	}
	function UTCToday(){
		var today = new Date();
		return UTCDate(today.getFullYear(), today.getMonth(), today.getDate());
	}
	function alias(method){
		return function(){
			return this[method].apply(this, arguments);
		};
	}

	var DateArray = (function(){
		var extras = {
			get: function(i){
				return this.slice(i)[0];
			},
			contains: function(d){
				// Array.indexOf is not cross-browser;
				// $.inArray doesn't work with Dates
				var val = d && d.valueOf();
				for (var i=0, l=this.length; i < l; i++)
					if (this[i].valueOf() === val)
						return i;
				return -1;
			},
			remove: function(i){
				this.splice(i,1);
			},
			replace: function(new_array){
				if (!new_array)
					return;
				if (!$.isArray(new_array))
					new_array = [new_array];
				this.clear();
				this.push.apply(this, new_array);
			},
			clear: function(){
				this.splice(0);
			},
			copy: function(){
				var a = new DateArray();
				a.replace(this);
				return a;
			}
		};

		return function(){
			var a = [];
			a.push.apply(a, arguments);
			$.extend(a, extras);
			return a;
		};
	})();


	// Picker object

	var Datepicker = function(element, options){
		this.dates = new DateArray();
		this.viewDate = UTCToday();
		this.focusDate = null;

		this._process_options(options);

		this.element = $(element);
		this.isInline = false;
		this.isInput = this.element.is('input');
		this.component = this.element.is('.date') ? this.element.find('.add-on, .input-group-addon, .btn') : false;
		this.hasInput = this.component && this.element.find('input').length;
		if (this.component && this.component.length === 0)
			this.component = false;

		this.picker = $(DPGlobal.template);
		this._buildEvents();
		this._attachEvents();

		if (this.isInline){
			this.picker.addClass('datepicker-inline').appendTo(this.element);
		}
		else {
			this.picker.addClass('datepicker-dropdown dropdown-menu');
		}

		if (this.o.rtl){
			this.picker.addClass('datepicker-rtl');
		}

		this.viewMode = this.o.startView;

		if (this.o.calendarWeeks)
			this.picker.find('tfoot th.today')
						.attr('colspan', function(i, val){
							return parseInt(val) + 1;
						});

		this._allow_update = false;

		this.setStartDate(this._o.startDate);
		this.setEndDate(this._o.endDate);
		this.setDaysOfWeekDisabled(this.o.daysOfWeekDisabled);

		this.fillDow();
		this.fillMonths();

		this._allow_update = true;

		this.update();
		this.showMode();

		if (this.isInline){
			this.show();
		}
	};

	Datepicker.prototype = {
		constructor: Datepicker,

		_process_options: function(opts){
			// Store raw options for reference
			this._o = $.extend({}, this._o, opts);
			// Processed options
			var o = this.o = $.extend({}, this._o);

			// Check if "de-DE" style date is available, if not language should
			// fallback to 2 letter code eg "de"
			var lang = o.language;
			if (!dates[lang]){
				lang = lang.split('-')[0];
				if (!dates[lang])
					lang = defaults.language;
			}
			o.language = lang;

			switch (o.startView){
				case 2:
				case 'decade':
					o.startView = 2;
					break;
				case 1:
				case 'year':
					o.startView = 1;
					break;
				default:
					o.startView = 0;
			}

			switch (o.minViewMode){
				case 1:
				case 'months':
					o.minViewMode = 1;
					break;
				case 2:
				case 'years':
					o.minViewMode = 2;
					break;
				default:
					o.minViewMode = 0;
			}

			o.startView = Math.max(o.startView, o.minViewMode);

			// true, false, or Number > 0
			if (o.multidate !== true){
				o.multidate = Number(o.multidate) || false;
				if (o.multidate !== false)
					o.multidate = Math.max(0, o.multidate);
				else
					o.multidate = 1;
			}
			o.multidateSeparator = String(o.multidateSeparator);

			o.weekStart %= 7;
			o.weekEnd = ((o.weekStart + 6) % 7);

			var format = DPGlobal.parseFormat(o.format);
			if (o.startDate !== -Infinity){
				if (!!o.startDate){
					if (o.startDate instanceof Date)
						o.startDate = this._local_to_utc(this._zero_time(o.startDate));
					else
						o.startDate = DPGlobal.parseDate(o.startDate, format, o.language);
				}
				else {
					o.startDate = -Infinity;
				}
			}
			if (o.endDate !== Infinity){
				if (!!o.endDate){
					if (o.endDate instanceof Date)
						o.endDate = this._local_to_utc(this._zero_time(o.endDate));
					else
						o.endDate = DPGlobal.parseDate(o.endDate, format, o.language);
				}
				else {
					o.endDate = Infinity;
				}
			}

			o.daysOfWeekDisabled = o.daysOfWeekDisabled||[];
			if (!$.isArray(o.daysOfWeekDisabled))
				o.daysOfWeekDisabled = o.daysOfWeekDisabled.split(/[,\s]*/);
			o.daysOfWeekDisabled = $.map(o.daysOfWeekDisabled, function(d){
				return parseInt(d, 10);
			});

			var plc = String(o.orientation).toLowerCase().split(/\s+/g),
				_plc = o.orientation.toLowerCase();
			plc = $.grep(plc, function(word){
				return (/^auto|left|right|top|bottom$/).test(word);
			});
			o.orientation = {x: 'auto', y: 'auto'};
			if (!_plc || _plc === 'auto')
				; // no action
			else if (plc.length === 1){
				switch (plc[0]){
					case 'top':
					case 'bottom':
						o.orientation.y = plc[0];
						break;
					case 'left':
					case 'right':
						o.orientation.x = plc[0];
						break;
				}
			}
			else {
				_plc = $.grep(plc, function(word){
					return (/^left|right$/).test(word);
				});
				o.orientation.x = _plc[0] || 'auto';

				_plc = $.grep(plc, function(word){
					return (/^top|bottom$/).test(word);
				});
				o.orientation.y = _plc[0] || 'auto';
			}
		},
		_events: [],
		_secondaryEvents: [],
		_applyEvents: function(evs){
			for (var i=0, el, ch, ev; i < evs.length; i++){
				el = evs[i][0];
				if (evs[i].length === 2){
					ch = undefined;
					ev = evs[i][1];
				}
				else if (evs[i].length === 3){
					ch = evs[i][1];
					ev = evs[i][2];
				}
				el.on(ev, ch);
			}
		},
		_unapplyEvents: function(evs){
			for (var i=0, el, ev, ch; i < evs.length; i++){
				el = evs[i][0];
				if (evs[i].length === 2){
					ch = undefined;
					ev = evs[i][1];
				}
				else if (evs[i].length === 3){
					ch = evs[i][1];
					ev = evs[i][2];
				}
				el.off(ev, ch);
			}
		},
		_buildEvents: function(){
			if (this.isInput){ // single input
				this._events = [
					[this.element, {
						focus: $.proxy(this.show, this),
						keyup: $.proxy(function(e){
							if ($.inArray(e.keyCode, [27,37,39,38,40,32,13,9]) === -1)
								this.update();
						}, this),
						keydown: $.proxy(this.keydown, this)
					}]
				];
			}
			else if (this.component && this.hasInput){ // component: input + button
				this._events = [
					// For components that are not readonly, allow keyboard nav
					[this.element.find('input'), {
						focus: $.proxy(this.show, this),
						keyup: $.proxy(function(e){
							if ($.inArray(e.keyCode, [27,37,39,38,40,32,13,9]) === -1)
								this.update();
						}, this),
						keydown: $.proxy(this.keydown, this)
					}],
					[this.component, {
						click: $.proxy(this.show, this)
					}]
				];
			}
			else if (this.element.is('div')){  // inline datepicker
				this.isInline = true;
			}
			else {
				this._events = [
					[this.element, {
						click: $.proxy(this.show, this)
					}]
				];
			}
			this._events.push(
				// Component: listen for blur on element descendants
				[this.element, '*', {
					blur: $.proxy(function(e){
						this._focused_from = e.target;
					}, this)
				}],
				// Input: listen for blur on element
				[this.element, {
					blur: $.proxy(function(e){
						this._focused_from = e.target;
					}, this)
				}]
			);

			this._secondaryEvents = [
				[this.picker, {
					click: $.proxy(this.click, this)
				}],
				[$(window), {
					resize: $.proxy(this.place, this)
				}],
				[$(document), {
					'mousedown touchstart': $.proxy(function(e){
						// Clicked outside the datepicker, hide it
						if (!(
							this.element.is(e.target) ||
							this.element.find(e.target).length ||
							this.picker.is(e.target) ||
							this.picker.find(e.target).length
						)){
							this.hide();
						}
					}, this)
				}]
			];
		},
		_attachEvents: function(){
			this._detachEvents();
			this._applyEvents(this._events);
		},
		_detachEvents: function(){
			this._unapplyEvents(this._events);
		},
		_attachSecondaryEvents: function(){
			this._detachSecondaryEvents();
			this._applyEvents(this._secondaryEvents);
		},
		_detachSecondaryEvents: function(){
			this._unapplyEvents(this._secondaryEvents);
		},
		_trigger: function(event, altdate){
			var date = altdate || this.dates.get(-1),
				local_date = this._utc_to_local(date);

			this.element.trigger({
				type: event,
				date: local_date,
				dates: $.map(this.dates, this._utc_to_local),
				format: $.proxy(function(ix, format){
					if (arguments.length === 0){
						ix = this.dates.length - 1;
						format = this.o.format;
					}
					else if (typeof ix === 'string'){
						format = ix;
						ix = this.dates.length - 1;
					}
					format = format || this.o.format;
					var date = this.dates.get(ix);
					return DPGlobal.formatDate(date, format, this.o.language);
				}, this)
			});
		},

		show: function(){
			if (!this.isInline)
				this.picker.appendTo('body');
			this.picker.show();
			this.place();
			this._attachSecondaryEvents();
			this._trigger('show');
		},

		hide: function(){
			if (this.isInline)
				return;
			if (!this.picker.is(':visible'))
				return;
			this.focusDate = null;
			this.picker.hide().detach();
			this._detachSecondaryEvents();
			this.viewMode = this.o.startView;
			this.showMode();

			if (
				this.o.forceParse &&
				(
					this.isInput && this.element.val() ||
					this.hasInput && this.element.find('input').val()
				)
			)
				this.setValue();
			this._trigger('hide');
		},

		remove: function(){
			this.hide();
			this._detachEvents();
			this._detachSecondaryEvents();
			this.picker.remove();
			delete this.element.data().datepicker;
			if (!this.isInput){
				delete this.element.data().date;
			}
		},

		_utc_to_local: function(utc){
			return utc && new Date(utc.getTime() + (utc.getTimezoneOffset()*60000));
		},
		_local_to_utc: function(local){
			return local && new Date(local.getTime() - (local.getTimezoneOffset()*60000));
		},
		_zero_time: function(local){
			return local && new Date(local.getFullYear(), local.getMonth(), local.getDate());
		},
		_zero_utc_time: function(utc){
			return utc && new Date(Date.UTC(utc.getUTCFullYear(), utc.getUTCMonth(), utc.getUTCDate()));
		},

		getDates: function(){
			return $.map(this.dates, this._utc_to_local);
		},

		getUTCDates: function(){
			return $.map(this.dates, function(d){
				return new Date(d);
			});
		},

		getDate: function(){
			return this._utc_to_local(this.getUTCDate());
		},

		getUTCDate: function(){
			return new Date(this.dates.get(-1));
		},

		setDates: function(){
			var args = $.isArray(arguments[0]) ? arguments[0] : arguments;
			this.update.apply(this, args);
			this._trigger('changeDate');
			this.setValue();
		},

		setUTCDates: function(){
			var args = $.isArray(arguments[0]) ? arguments[0] : arguments;
			this.update.apply(this, $.map(args, this._utc_to_local));
			this._trigger('changeDate');
			this.setValue();
		},

		setDate: alias('setDates'),
		setUTCDate: alias('setUTCDates'),

		setValue: function(){
			var formatted = this.getFormattedDate();
			if (!this.isInput){
				if (this.component){
					this.element.find('input').val(formatted).change();
				}
			}
			else {
				this.element.val(formatted).change();
			}
		},

		getFormattedDate: function(format){
			if (format === undefined)
				format = this.o.format;

			var lang = this.o.language;
			return $.map(this.dates, function(d){
				return DPGlobal.formatDate(d, format, lang);
			}).join(this.o.multidateSeparator);
		},

		setStartDate: function(startDate){
			this._process_options({startDate: startDate});
			this.update();
			this.updateNavArrows();
		},

		setEndDate: function(endDate){
			this._process_options({endDate: endDate});
			this.update();
			this.updateNavArrows();
		},

		setDaysOfWeekDisabled: function(daysOfWeekDisabled){
			this._process_options({daysOfWeekDisabled: daysOfWeekDisabled});
			this.update();
			this.updateNavArrows();
		},

		place: function(){
			if (this.isInline)
				return;
			var calendarWidth = this.picker.outerWidth(),
				calendarHeight = this.picker.outerHeight(),
				visualPadding = 10,
				windowWidth = $window.width(),
				windowHeight = $window.height(),
				scrollTop = $window.scrollTop();

			var zIndex = parseInt(this.element.parents().filter(function(){
					return $(this).css('z-index') !== 'auto';
				}).first().css('z-index'))+10;
			var offset = this.component ? this.component.parent().offset() : this.element.offset();
			var height = this.component ? this.component.outerHeight(true) : this.element.outerHeight(false);
			var width = this.component ? this.component.outerWidth(true) : this.element.outerWidth(false);
			var left = offset.left,
				top = offset.top;

			this.picker.removeClass(
				'datepicker-orient-top datepicker-orient-bottom '+
				'datepicker-orient-right datepicker-orient-left'
			);

			if (this.o.orientation.x !== 'auto'){
				this.picker.addClass('datepicker-orient-' + this.o.orientation.x);
				if (this.o.orientation.x === 'right')
					left -= calendarWidth - width;
			}
			// auto x orientation is best-placement: if it crosses a window
			// edge, fudge it sideways
			else {
				// Default to left
				this.picker.addClass('datepicker-orient-left');
				if (offset.left < 0)
					left -= offset.left - visualPadding;
				else if (offset.left + calendarWidth > windowWidth)
					left = windowWidth - calendarWidth - visualPadding;
			}

			// auto y orientation is best-situation: top or bottom, no fudging,
			// decision based on which shows more of the calendar
			var yorient = this.o.orientation.y,
				top_overflow, bottom_overflow;
			if (yorient === 'auto'){
				top_overflow = -scrollTop + offset.top - calendarHeight;
				bottom_overflow = scrollTop + windowHeight - (offset.top + height + calendarHeight);
				if (Math.max(top_overflow, bottom_overflow) === bottom_overflow)
					yorient = 'top';
				else
					yorient = 'bottom';
			}
			this.picker.addClass('datepicker-orient-' + yorient);
			if (yorient === 'top')
				top += height;
			else
				top -= calendarHeight + parseInt(this.picker.css('padding-top'));

			this.picker.css({
				top: top,
				left: left,
				zIndex: zIndex
			});
		},

		_allow_update: true,
		update: function(){
			if (!this._allow_update)
				return;

			var oldDates = this.dates.copy(),
				dates = [],
				fromArgs = false;
			if (arguments.length){
				$.each(arguments, $.proxy(function(i, date){
					if (date instanceof Date)
						date = this._local_to_utc(date);
					dates.push(date);
				}, this));
				fromArgs = true;
			}
			else {
				dates = this.isInput
						? this.element.val()
						: this.element.data('date') || this.element.find('input').val();
				if (dates && this.o.multidate)
					dates = dates.split(this.o.multidateSeparator);
				else
					dates = [dates];
				delete this.element.data().date;
			}

			dates = $.map(dates, $.proxy(function(date){
				return DPGlobal.parseDate(date, this.o.format, this.o.language);
			}, this));
			dates = $.grep(dates, $.proxy(function(date){
				return (
					date < this.o.startDate ||
					date > this.o.endDate ||
					!date
				);
			}, this), true);
			this.dates.replace(dates);

			if (this.dates.length)
				this.viewDate = new Date(this.dates.get(-1));
			else if (this.viewDate < this.o.startDate)
				this.viewDate = new Date(this.o.startDate);
			else if (this.viewDate > this.o.endDate)
				this.viewDate = new Date(this.o.endDate);

			if (fromArgs){
				// setting date by clicking
				this.setValue();
			}
			else if (dates.length){
				// setting date by typing
				if (String(oldDates) !== String(this.dates))
					this._trigger('changeDate');
			}
			if (!this.dates.length && oldDates.length)
				this._trigger('clearDate');

			this.fill();
		},

		fillDow: function(){
			var dowCnt = this.o.weekStart,
				html = '<tr>';
			if (this.o.calendarWeeks){
				var cell = '<th class="cw">&nbsp;</th>';
				html += cell;
				this.picker.find('.datepicker-days thead tr:first-child').prepend(cell);
			}
			while (dowCnt < this.o.weekStart + 7){
				html += '<th class="dow">'+dates[this.o.language].daysMin[(dowCnt++)%7]+'</th>';
			}
			html += '</tr>';
			this.picker.find('.datepicker-days thead').append(html);
		},

		fillMonths: function(){
			var html = '',
			i = 0;
			while (i < 12){
				html += '<span class="month">'+dates[this.o.language].monthsShort[i++]+'</span>';
			}
			this.picker.find('.datepicker-months td').html(html);
		},

		setRange: function(range){
			if (!range || !range.length)
				delete this.range;
			else
				this.range = $.map(range, function(d){
					return d.valueOf();
				});
			this.fill();
		},

		getClassNames: function(date){
			var cls = [],
				year = this.viewDate.getUTCFullYear(),
				month = this.viewDate.getUTCMonth(),
				today = new Date();
			if (date.getUTCFullYear() < year || (date.getUTCFullYear() === year && date.getUTCMonth() < month)){
				cls.push('old');
			}
			else if (date.getUTCFullYear() > year || (date.getUTCFullYear() === year && date.getUTCMonth() > month)){
				cls.push('new');
			}
			if (this.focusDate && date.valueOf() === this.focusDate.valueOf())
				cls.push('focused');
			// Compare internal UTC date with local today, not UTC today
			if (this.o.todayHighlight &&
				date.getUTCFullYear() === today.getFullYear() &&
				date.getUTCMonth() === today.getMonth() &&
				date.getUTCDate() === today.getDate()){
				cls.push('today');
			}
			if (this.dates.contains(date) !== -1)
				cls.push('active');
			if (date.valueOf() < this.o.startDate || date.valueOf() > this.o.endDate ||
				$.inArray(date.getUTCDay(), this.o.daysOfWeekDisabled) !== -1){
				cls.push('disabled');
			}
			if (this.range){
				if (date > this.range[0] && date < this.range[this.range.length-1]){
					cls.push('range');
				}
				if ($.inArray(date.valueOf(), this.range) !== -1){
					cls.push('selected');
				}
			}
			return cls;
		},

		fill: function(){
			var d = new Date(this.viewDate),
				year = d.getUTCFullYear(),
				month = d.getUTCMonth(),
				startYear = this.o.startDate !== -Infinity ? this.o.startDate.getUTCFullYear() : -Infinity,
				startMonth = this.o.startDate !== -Infinity ? this.o.startDate.getUTCMonth() : -Infinity,
				endYear = this.o.endDate !== Infinity ? this.o.endDate.getUTCFullYear() : Infinity,
				endMonth = this.o.endDate !== Infinity ? this.o.endDate.getUTCMonth() : Infinity,
				todaytxt = dates[this.o.language].today || dates['en'].today || '',
				cleartxt = dates[this.o.language].clear || dates['en'].clear || '',
				tooltip;
			this.picker.find('.datepicker-days thead th.datepicker-switch')
						.text(dates[this.o.language].months[month]+' '+year);
			this.picker.find('tfoot th.today')
						.text(todaytxt)
						.toggle(this.o.todayBtn !== false);
			this.picker.find('tfoot th.clear')
						.text(cleartxt)
						.toggle(this.o.clearBtn !== false);
			this.updateNavArrows();
			this.fillMonths();
			var prevMonth = UTCDate(year, month-1, 28),
				day = DPGlobal.getDaysInMonth(prevMonth.getUTCFullYear(), prevMonth.getUTCMonth());
			prevMonth.setUTCDate(day);
			prevMonth.setUTCDate(day - (prevMonth.getUTCDay() - this.o.weekStart + 7)%7);
			var nextMonth = new Date(prevMonth);
			nextMonth.setUTCDate(nextMonth.getUTCDate() + 42);
			nextMonth = nextMonth.valueOf();
			var html = [];
			var clsName;
			while (prevMonth.valueOf() < nextMonth){
				if (prevMonth.getUTCDay() === this.o.weekStart){
					html.push('<tr>');
					if (this.o.calendarWeeks){
						// ISO 8601: First week contains first thursday.
						// ISO also states week starts on Monday, but we can be more abstract here.
						var
							// Start of current week: based on weekstart/current date
							ws = new Date(+prevMonth + (this.o.weekStart - prevMonth.getUTCDay() - 7) % 7 * 864e5),
							// Thursday of this week
							th = new Date(Number(ws) + (7 + 4 - ws.getUTCDay()) % 7 * 864e5),
							// First Thursday of year, year from thursday
							yth = new Date(Number(yth = UTCDate(th.getUTCFullYear(), 0, 1)) + (7 + 4 - yth.getUTCDay())%7*864e5),
							// Calendar week: ms between thursdays, div ms per day, div 7 days
							calWeek =  (th - yth) / 864e5 / 7 + 1;
						html.push('<td class="cw">'+ calWeek +'</td>');

					}
				}
				clsName = this.getClassNames(prevMonth);
				clsName.push('day');

				if (this.o.beforeShowDay !== $.noop){
					var before = this.o.beforeShowDay(this._utc_to_local(prevMonth));
					if (before === undefined)
						before = {};
					else if (typeof(before) === 'boolean')
						before = {enabled: before};
					else if (typeof(before) === 'string')
						before = {classes: before};
					if (before.enabled === false)
						clsName.push('disabled');
					if (before.classes)
						clsName = clsName.concat(before.classes.split(/\s+/));
					if (before.tooltip)
						tooltip = before.tooltip;
				}

				clsName = $.unique(clsName);
				html.push('<td class="'+clsName.join(' ')+'"' + (tooltip ? ' title="'+tooltip+'"' : '') + '>'+prevMonth.getUTCDate() + '</td>');
				if (prevMonth.getUTCDay() === this.o.weekEnd){
					html.push('</tr>');
				}
				prevMonth.setUTCDate(prevMonth.getUTCDate()+1);
			}
			this.picker.find('.datepicker-days tbody').empty().append(html.join(''));

			var months = this.picker.find('.datepicker-months')
						.find('th:eq(1)')
							.text(year)
							.end()
						.find('span').removeClass('active');

			$.each(this.dates, function(i, d){
				if (d.getUTCFullYear() === year)
					months.eq(d.getUTCMonth()).addClass('active');
			});

			if (year < startYear || year > endYear){
				months.addClass('disabled');
			}
			if (year === startYear){
				months.slice(0, startMonth).addClass('disabled');
			}
			if (year === endYear){
				months.slice(endMonth+1).addClass('disabled');
			}

			html = '';
			year = parseInt(year/10, 10) * 10;
			var yearCont = this.picker.find('.datepicker-years')
								.find('th:eq(1)')
									.text(year + '-' + (year + 9))
									.end()
								.find('td');
			year -= 1;
			var years = $.map(this.dates, function(d){
					return d.getUTCFullYear();
				}),
				classes;
			for (var i = -1; i < 11; i++){
				classes = ['year'];
				if (i === -1)
					classes.push('old');
				else if (i === 10)
					classes.push('new');
				if ($.inArray(year, years) !== -1)
					classes.push('active');
				if (year < startYear || year > endYear)
					classes.push('disabled');
				html += '<span class="' + classes.join(' ') + '">'+year+'</span>';
				year += 1;
			}
			yearCont.html(html);
		},

		updateNavArrows: function(){
			if (!this._allow_update)
				return;

			var d = new Date(this.viewDate),
				year = d.getUTCFullYear(),
				month = d.getUTCMonth();
			switch (this.viewMode){
				case 0:
					if (this.o.startDate !== -Infinity && year <= this.o.startDate.getUTCFullYear() && month <= this.o.startDate.getUTCMonth()){
						this.picker.find('.prev').css({visibility: 'hidden'});
					}
					else {
						this.picker.find('.prev').css({visibility: 'visible'});
					}
					if (this.o.endDate !== Infinity && year >= this.o.endDate.getUTCFullYear() && month >= this.o.endDate.getUTCMonth()){
						this.picker.find('.next').css({visibility: 'hidden'});
					}
					else {
						this.picker.find('.next').css({visibility: 'visible'});
					}
					break;
				case 1:
				case 2:
					if (this.o.startDate !== -Infinity && year <= this.o.startDate.getUTCFullYear()){
						this.picker.find('.prev').css({visibility: 'hidden'});
					}
					else {
						this.picker.find('.prev').css({visibility: 'visible'});
					}
					if (this.o.endDate !== Infinity && year >= this.o.endDate.getUTCFullYear()){
						this.picker.find('.next').css({visibility: 'hidden'});
					}
					else {
						this.picker.find('.next').css({visibility: 'visible'});
					}
					break;
			}
		},

		click: function(e){
			e.preventDefault();
			var target = $(e.target).closest('span, td, th'),
				year, month, day;
			if (target.length === 1){
				switch (target[0].nodeName.toLowerCase()){
					case 'th':
						switch (target[0].className){
							case 'datepicker-switch':
								this.showMode(1);
								break;
							case 'prev':
							case 'next':
								var dir = DPGlobal.modes[this.viewMode].navStep * (target[0].className === 'prev' ? -1 : 1);
								switch (this.viewMode){
									case 0:
										this.viewDate = this.moveMonth(this.viewDate, dir);
										this._trigger('changeMonth', this.viewDate);
										break;
									case 1:
									case 2:
										this.viewDate = this.moveYear(this.viewDate, dir);
										if (this.viewMode === 1)
											this._trigger('changeYear', this.viewDate);
										break;
								}
								this.fill();
								break;
							case 'today':
								var date = new Date();
								date = UTCDate(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0);

								this.showMode(-2);
								var which = this.o.todayBtn === 'linked' ? null : 'view';
								this._setDate(date, which);
								break;
							case 'clear':
								var element;
								if (this.isInput)
									element = this.element;
								else if (this.component)
									element = this.element.find('input');
								if (element)
									element.val("").change();
								this.update();
								this._trigger('changeDate');
								if (this.o.autoclose)
									this.hide();
								break;
						}
						break;
					case 'span':
						if (!target.is('.disabled')){
							this.viewDate.setUTCDate(1);
							if (target.is('.month')){
								day = 1;
								month = target.parent().find('span').index(target);
								year = this.viewDate.getUTCFullYear();
								this.viewDate.setUTCMonth(month);
								this._trigger('changeMonth', this.viewDate);
								if (this.o.minViewMode === 1){
									this._setDate(UTCDate(year, month, day));
								}
							}
							else {
								day = 1;
								month = 0;
								year = parseInt(target.text(), 10)||0;
								this.viewDate.setUTCFullYear(year);
								this._trigger('changeYear', this.viewDate);
								if (this.o.minViewMode === 2){
									this._setDate(UTCDate(year, month, day));
								}
							}
							this.showMode(-1);
							this.fill();
						}
						break;
					case 'td':
						if (target.is('.day') && !target.is('.disabled')){
							day = parseInt(target.text(), 10)||1;
							year = this.viewDate.getUTCFullYear();
							month = this.viewDate.getUTCMonth();
							if (target.is('.old')){
								if (month === 0){
									month = 11;
									year -= 1;
								}
								else {
									month -= 1;
								}
							}
							else if (target.is('.new')){
								if (month === 11){
									month = 0;
									year += 1;
								}
								else {
									month += 1;
								}
							}
							this._setDate(UTCDate(year, month, day));
						}
						break;
				}
			}
			if (this.picker.is(':visible') && this._focused_from){
				$(this._focused_from).focus();
			}
			delete this._focused_from;
		},

		_toggle_multidate: function(date){
			var ix = this.dates.contains(date);
			if (!date){
				this.dates.clear();
			}
			else if (ix !== -1 && this.o.allowDeselection) {
				this.dates.remove(ix);
			}
			else {
				this.dates.push(date);
			}
			if (typeof this.o.multidate === 'number')
				while (this.dates.length > this.o.multidate)
					this.dates.remove(0);
		},

		_setDate: function(date, which){
			if (!which || which === 'date')
				this._toggle_multidate(date && new Date(date));
			if (!which || which  === 'view')
				this.viewDate = date && new Date(date);

			this.fill();
			this.setValue();
			this._trigger('changeDate');
			var element;
			if (this.isInput){
				element = this.element;
			}
			else if (this.component){
				element = this.element.find('input');
			}
			if (element){
				element.change();
			}
			if (this.o.autoclose && (!which || which === 'date')){
				this.hide();
			}
		},

		moveMonth: function(date, dir){
			if (!date)
				return undefined;
			if (!dir)
				return date;
			var new_date = new Date(date.valueOf()),
				day = new_date.getUTCDate(),
				month = new_date.getUTCMonth(),
				mag = Math.abs(dir),
				new_month, test;
			dir = dir > 0 ? 1 : -1;
			if (mag === 1){
				test = dir === -1
					// If going back one month, make sure month is not current month
					// (eg, Mar 31 -> Feb 31 == Feb 28, not Mar 02)
					? function(){
						return new_date.getUTCMonth() === month;
					}
					// If going forward one month, make sure month is as expected
					// (eg, Jan 31 -> Feb 31 == Feb 28, not Mar 02)
					: function(){
						return new_date.getUTCMonth() !== new_month;
					};
				new_month = month + dir;
				new_date.setUTCMonth(new_month);
				// Dec -> Jan (12) or Jan -> Dec (-1) -- limit expected date to 0-11
				if (new_month < 0 || new_month > 11)
					new_month = (new_month + 12) % 12;
			}
			else {
				// For magnitudes >1, move one month at a time...
				for (var i=0; i < mag; i++)
					// ...which might decrease the day (eg, Jan 31 to Feb 28, etc)...
					new_date = this.moveMonth(new_date, dir);
				// ...then reset the day, keeping it in the new month
				new_month = new_date.getUTCMonth();
				new_date.setUTCDate(day);
				test = function(){
					return new_month !== new_date.getUTCMonth();
				};
			}
			// Common date-resetting loop -- if date is beyond end of month, make it
			// end of month
			while (test()){
				new_date.setUTCDate(--day);
				new_date.setUTCMonth(new_month);
			}
			return new_date;
		},

		moveYear: function(date, dir){
			return this.moveMonth(date, dir*12);
		},

		dateWithinRange: function(date){
			return date >= this.o.startDate && date <= this.o.endDate;
		},

		keydown: function(e){
			if (this.picker.is(':not(:visible)')){
				if (e.keyCode === 27) // allow escape to hide and re-show picker
					this.show();
				return;
			}
			var dateChanged = false,
				dir, newDate, newViewDate,
				focusDate = this.focusDate || this.viewDate;
			switch (e.keyCode){
				case 27: // escape
					if (this.focusDate){
						this.focusDate = null;
						this.viewDate = this.dates.get(-1) || this.viewDate;
						this.fill();
					}
					else
						this.hide();
					e.preventDefault();
					break;
				case 37: // left
				case 39: // right
					if (!this.o.keyboardNavigation)
						break;
					dir = e.keyCode === 37 ? -1 : 1;
					if (e.ctrlKey){
						newDate = this.moveYear(this.dates.get(-1) || UTCToday(), dir);
						newViewDate = this.moveYear(focusDate, dir);
						this._trigger('changeYear', this.viewDate);
					}
					else if (e.shiftKey){
						newDate = this.moveMonth(this.dates.get(-1) || UTCToday(), dir);
						newViewDate = this.moveMonth(focusDate, dir);
						this._trigger('changeMonth', this.viewDate);
					}
					else {
						newDate = new Date(this.dates.get(-1) || UTCToday());
						newDate.setUTCDate(newDate.getUTCDate() + dir);
						newViewDate = new Date(focusDate);
						newViewDate.setUTCDate(focusDate.getUTCDate() + dir);
					}
					if (this.dateWithinRange(newDate)){
						this.focusDate = this.viewDate = newViewDate;
						this.setValue();
						this.fill();
						e.preventDefault();
					}
					break;
				case 38: // up
				case 40: // down
					if (!this.o.keyboardNavigation)
						break;
					dir = e.keyCode === 38 ? -1 : 1;
					if (e.ctrlKey){
						newDate = this.moveYear(this.dates.get(-1) || UTCToday(), dir);
						newViewDate = this.moveYear(focusDate, dir);
						this._trigger('changeYear', this.viewDate);
					}
					else if (e.shiftKey){
						newDate = this.moveMonth(this.dates.get(-1) || UTCToday(), dir);
						newViewDate = this.moveMonth(focusDate, dir);
						this._trigger('changeMonth', this.viewDate);
					}
					else {
						newDate = new Date(this.dates.get(-1) || UTCToday());
						newDate.setUTCDate(newDate.getUTCDate() + dir * 7);
						newViewDate = new Date(focusDate);
						newViewDate.setUTCDate(focusDate.getUTCDate() + dir * 7);
					}
					if (this.dateWithinRange(newDate)){
						this.focusDate = this.viewDate = newViewDate;
						this.setValue();
						this.fill();
						e.preventDefault();
					}
					break;
				case 32: // spacebar
					// Spacebar is used in manually typing dates in some formats.
					// As such, its behavior should not be hijacked.
					break;
				case 13: // enter
					focusDate = this.focusDate || this.dates.get(-1) || this.viewDate;
					this._toggle_multidate(focusDate);
					dateChanged = true;
					this.focusDate = null;
					this.viewDate = this.dates.get(-1) || this.viewDate;
					this.setValue();
					this.fill();
					if (this.picker.is(':visible')){
						e.preventDefault();
						if (this.o.autoclose)
							this.hide();
					}
					break;
				case 9: // tab
					this.focusDate = null;
					this.viewDate = this.dates.get(-1) || this.viewDate;
					this.fill();
					this.hide();
					break;
			}
			if (dateChanged){
				if (this.dates.length)
					this._trigger('changeDate');
				else
					this._trigger('clearDate');
				var element;
				if (this.isInput){
					element = this.element;
				}
				else if (this.component){
					element = this.element.find('input');
				}
				if (element){
					element.change();
				}
			}
		},

		showMode: function(dir){
			if (dir){
				this.viewMode = Math.max(this.o.minViewMode, Math.min(2, this.viewMode + dir));
			}
			this.picker
				.find('>div')
				.hide()
				.filter('.datepicker-'+DPGlobal.modes[this.viewMode].clsName)
					.css('display', 'block');
			this.updateNavArrows();
		}
	};

	var DateRangePicker = function(element, options){
		this.element = $(element);
		this.inputs = $.map(options.inputs, function(i){
			return i.jquery ? i[0] : i;
		});
		delete options.inputs;

		$(this.inputs)
			.datepicker(options)
			.bind('changeDate', $.proxy(this.dateUpdated, this));

		this.pickers = $.map(this.inputs, function(i){
			return $(i).data('datepicker');
		});
		this.updateDates();
	};
	DateRangePicker.prototype = {
		updateDates: function(){
			this.dates = $.map(this.pickers, function(i){
				return i.getUTCDate();
			});
			this.updateRanges();
		},
		updateRanges: function(){
			var range = $.map(this.dates, function(d){
				return d.valueOf();
			});
			$.each(this.pickers, function(i, p){
				p.setRange(range);
			});
		},
		dateUpdated: function(e){
			// `this.updating` is a workaround for preventing infinite recursion
			// between `changeDate` triggering and `setUTCDate` calling.  Until
			// there is a better mechanism.
			if (this.updating)
				return;
			this.updating = true;

			var dp = $(e.target).data('datepicker'),
				new_date = dp.getUTCDate(),
				i = $.inArray(e.target, this.inputs),
				l = this.inputs.length;
			if (i === -1)
				return;

			$.each(this.pickers, function(i, p){
				if (!p.getUTCDate())
					p.setUTCDate(new_date);
			});

			if (new_date < this.dates[i]){
				// Date being moved earlier/left
				while (i >= 0 && new_date < this.dates[i]){
					this.pickers[i--].setUTCDate(new_date);
				}
			}
			else if (new_date > this.dates[i]){
				// Date being moved later/right
				while (i < l && new_date > this.dates[i]){
					this.pickers[i++].setUTCDate(new_date);
				}
			}
			this.updateDates();

			delete this.updating;
		},
		remove: function(){
			$.map(this.pickers, function(p){ p.remove(); });
			delete this.element.data().datepicker;
		}
	};

	function opts_from_el(el, prefix){
		// Derive options from element data-attrs
		var data = $(el).data(),
			out = {}, inkey,
			replace = new RegExp('^' + prefix.toLowerCase() + '([A-Z])');
		prefix = new RegExp('^' + prefix.toLowerCase());
		function re_lower(_,a){
			return a.toLowerCase();
		}
		for (var key in data)
			if (prefix.test(key)){
				inkey = key.replace(replace, re_lower);
				out[inkey] = data[key];
			}
		return out;
	}

	function opts_from_locale(lang){
		// Derive options from locale plugins
		var out = {};
		// Check if "de-DE" style date is available, if not language should
		// fallback to 2 letter code eg "de"
		if (!dates[lang]){
			lang = lang.split('-')[0];
			if (!dates[lang])
				return;
		}
		var d = dates[lang];
		$.each(locale_opts, function(i,k){
			if (k in d)
				out[k] = d[k];
		});
		return out;
	}

	var old = $.fn.datepicker;
	$.fn.datepicker = function(option){
		var args = Array.apply(null, arguments);
		args.shift();
		var internal_return;
		this.each(function(){
			var $this = $(this),
				data = $this.data('datepicker'),
				options = typeof option === 'object' && option;
			if (!data){
				var elopts = opts_from_el(this, 'date'),
					// Preliminary otions
					xopts = $.extend({}, defaults, elopts, options),
					locopts = opts_from_locale(xopts.language),
					// Options priority: js args, data-attrs, locales, defaults
					opts = $.extend({}, defaults, locopts, elopts, options);
				if ($this.is('.input-daterange') || opts.inputs){
					var ropts = {
						inputs: opts.inputs || $this.find('input').toArray()
					};
					$this.data('datepicker', (data = new DateRangePicker(this, $.extend(opts, ropts))));
				}
				else {
					$this.data('datepicker', (data = new Datepicker(this, opts)));
				}
			}
			if (typeof option === 'string' && typeof data[option] === 'function'){
				internal_return = data[option].apply(data, args);
				if (internal_return !== undefined)
					return false;
			}
		});
		if (internal_return !== undefined)
			return internal_return;
		else
			return this;
	};

	var defaults = $.fn.datepicker.defaults = {
	    allowDeselection: false,
		autoclose: true,
		beforeShowDay: $.noop,
		calendarWeeks: false,
		clearBtn: false,
		daysOfWeekDisabled: [],
		endDate: Infinity,
		forceParse: true,
		format: 'mm/dd/yyyy',
		keyboardNavigation: true,
		language: 'en',
		minViewMode: 0,
		multidate: false,
		multidateSeparator: ',',
		orientation: "auto",
		rtl: false,
		startDate: -Infinity,
		startView: 0,
		todayBtn: false,
		todayHighlight: false,
		weekStart: 0
	};
	var locale_opts = $.fn.datepicker.locale_opts = [
		'format',
		'rtl',
		'weekStart'
	];
	$.fn.datepicker.Constructor = Datepicker;
	var dates = $.fn.datepicker.dates = {
		en: {
			days: ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"],
			daysShort: ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"],
			daysMin: ["Su", "Mo", "Tu", "We", "Th", "Fr", "Sa", "Su"],
			months: ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"],
			monthsShort: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"],
			today: "Today",
			clear: "Clear"
		}
	};

	var DPGlobal = {
		modes: [
			{
				clsName: 'days',
				navFnc: 'Month',
				navStep: 1
			},
			{
				clsName: 'months',
				navFnc: 'FullYear',
				navStep: 1
			},
			{
				clsName: 'years',
				navFnc: 'FullYear',
				navStep: 10
		}],
		isLeapYear: function(year){
			return (((year % 4 === 0) && (year % 100 !== 0)) || (year % 400 === 0));
		},
		getDaysInMonth: function(year, month){
			return [31, (DPGlobal.isLeapYear(year) ? 29 : 28), 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][month];
		},
		validParts: /dd?|DD?|mm?|MM?|yy(?:yy)?/g,
		nonpunctuation: /[^ -\/:-@\[\u3400-\u9fff-`{-~\t\n\r]+/g,
		parseFormat: function(format){
			// IE treats \0 as a string end in inputs (truncating the value),
			// so it's a bad format delimiter, anyway
			var separators = format.replace(this.validParts, '\0').split('\0'),
				parts = format.match(this.validParts);
			if (!separators || !separators.length || !parts || parts.length === 0){
				throw new Error("Invalid date format.");
			}
			return {separators: separators, parts: parts};
		},
		parseDate: function(date, format, language){
			if (!date)
				return undefined;
			if (date instanceof Date)
				return date;
			if (typeof format === 'string')
				format = DPGlobal.parseFormat(format);
			var part_re = /([\-+]\d+)([dmwy])/,
				parts = date.match(/([\-+]\d+)([dmwy])/g),
				part, dir, i;
			if (/^[\-+]\d+[dmwy]([\s,]+[\-+]\d+[dmwy])*$/.test(date)){
				date = new Date();
				for (i=0; i < parts.length; i++){
					part = part_re.exec(parts[i]);
					dir = parseInt(part[1]);
					switch (part[2]){
						case 'd':
							date.setUTCDate(date.getUTCDate() + dir);
							break;
						case 'm':
							date = Datepicker.prototype.moveMonth.call(Datepicker.prototype, date, dir);
							break;
						case 'w':
							date.setUTCDate(date.getUTCDate() + dir * 7);
							break;
						case 'y':
							date = Datepicker.prototype.moveYear.call(Datepicker.prototype, date, dir);
							break;
					}
				}
				return UTCDate(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate(), 0, 0, 0);
			}
			parts = date && date.match(this.nonpunctuation) || [];
			date = new Date();
			var parsed = {},
				setters_order = ['yyyy', 'yy', 'M', 'MM', 'm', 'mm', 'd', 'dd'],
				setters_map = {
					yyyy: function(d,v){
						return d.setUTCFullYear(v);
					},
					yy: function(d,v){
						return d.setUTCFullYear(2000+v);
					},
					m: function(d,v){
						if (isNaN(d))
							return d;
						v -= 1;
						while (v < 0) v += 12;
						v %= 12;
						d.setUTCMonth(v);
						while (d.getUTCMonth() !== v)
							d.setUTCDate(d.getUTCDate()-1);
						return d;
					},
					d: function(d,v){
						return d.setUTCDate(v);
					}
				},
				val, filtered;
			setters_map['M'] = setters_map['MM'] = setters_map['mm'] = setters_map['m'];
			setters_map['dd'] = setters_map['d'];
			date = UTCDate(date.getFullYear(), date.getMonth(), date.getDate(), 0, 0, 0);
			var fparts = format.parts.slice();
			// Remove noop parts
			if (parts.length !== fparts.length){
				fparts = $(fparts).filter(function(i,p){
					return $.inArray(p, setters_order) !== -1;
				}).toArray();
			}
			// Process remainder
			function match_part(){
				var m = this.slice(0, parts[i].length),
					p = parts[i].slice(0, m.length);
				return m === p;
			}
			if (parts.length === fparts.length){
				var cnt;
				for (i=0, cnt = fparts.length; i < cnt; i++){
					val = parseInt(parts[i], 10);
					part = fparts[i];
					if (isNaN(val)){
						switch (part){
							case 'MM':
								filtered = $(dates[language].months).filter(match_part);
								val = $.inArray(filtered[0], dates[language].months) + 1;
								break;
							case 'M':
								filtered = $(dates[language].monthsShort).filter(match_part);
								val = $.inArray(filtered[0], dates[language].monthsShort) + 1;
								break;
						}
					}
					parsed[part] = val;
				}
				var _date, s;
				for (i=0; i < setters_order.length; i++){
					s = setters_order[i];
					if (s in parsed && !isNaN(parsed[s])){
						_date = new Date(date);
						setters_map[s](_date, parsed[s]);
						if (!isNaN(_date))
							date = _date;
					}
				}
			}
			return date;
		},
		formatDate: function(date, format, language){
			if (!date)
				return '';
			if (typeof format === 'string')
				format = DPGlobal.parseFormat(format);
			var val = {
				d: date.getUTCDate(),
				D: dates[language].daysShort[date.getUTCDay()],
				DD: dates[language].days[date.getUTCDay()],
				m: date.getUTCMonth() + 1,
				M: dates[language].monthsShort[date.getUTCMonth()],
				MM: dates[language].months[date.getUTCMonth()],
				yy: date.getUTCFullYear().toString().substring(2),
				yyyy: date.getUTCFullYear()
			};
			val.dd = (val.d < 10 ? '0' : '') + val.d;
			val.mm = (val.m < 10 ? '0' : '') + val.m;
			date = [];
			var seps = $.extend([], format.separators);
			for (var i=0, cnt = format.parts.length; i <= cnt; i++){
				if (seps.length)
					date.push(seps.shift());
				date.push(val[format.parts[i]]);
			}
			return date.join('');
		},
		headTemplate: '<thead>'+
							'<tr>'+
								'<th class="prev">&laquo;</th>'+
								'<th colspan="5" class="datepicker-switch"></th>'+
								'<th class="next">&raquo;</th>'+
							'</tr>'+
						'</thead>',
		contTemplate: '<tbody><tr><td colspan="7"></td></tr></tbody>',
		footTemplate: '<tfoot>'+
							'<tr>'+
								'<th colspan="7" class="today"></th>'+
							'</tr>'+
							'<tr>'+
								'<th colspan="7" class="clear"></th>'+
							'</tr>'+
						'</tfoot>'
	};
	DPGlobal.template = '<div class="datepicker">'+
							'<div class="datepicker-days">'+
								'<table class=" table-condensed">'+
									DPGlobal.headTemplate+
									'<tbody></tbody>'+
									DPGlobal.footTemplate+
								'</table>'+
							'</div>'+
							'<div class="datepicker-months">'+
								'<table class="table-condensed">'+
									DPGlobal.headTemplate+
									DPGlobal.contTemplate+
									DPGlobal.footTemplate+
								'</table>'+
							'</div>'+
							'<div class="datepicker-years">'+
								'<table class="table-condensed">'+
									DPGlobal.headTemplate+
									DPGlobal.contTemplate+
									DPGlobal.footTemplate+
								'</table>'+
							'</div>'+
						'</div>';

	$.fn.datepicker.DPGlobal = DPGlobal;


	/* DATEPICKER NO CONFLICT
	* =================== */

	$.fn.datepicker.noConflict = function(){
		$.fn.datepicker = old;
		return this;
	};


	/* DATEPICKER DATA-API
	* ================== */

	$(document).on(
		'focus.datepicker.data-api click.datepicker.data-api',
		'[data-provide="datepicker"]',
		function(e){
			var $this = $(this);
			if ($this.data('datepicker'))
				return;
			e.preventDefault();
			// component click requires us to explicitly show it
			$this.datepicker('show');
		}
	);
	$(function(){
		$('[data-provide="datepicker-inline"]').datepicker();
	});

}(window.jQuery));


mutate_event_stack = [
			{
				name: 'width',
				handler: function (elem){
					n = {el:elem}
					if(!$(n.el).data('mutate-width'))$(n.el).data('mutate-width', $(n.el).width());
					if ($(n.el).data('mutate-width')&&$(n.el).width() != $(n.el).data('mutate-width')  ) {
						$(n.el).data('mutate-width', $(n.el).width());
						return true;
					}
					return false;
				}
			},
			{
				name:'height',
				handler: function (n){
					element = n;
					if(!$(element).data('mutate-height'))$(element).data('mutate-height', $(element).height());
					if ($(element).data('mutate-height')&&$(element).height() != $(element).data('mutate-height')  ) {
						$(element).data('mutate-height', $(element).height());
						return true;
					}
				}
			},
			{
				name		: 'top',
				handler 	: function (n){
					if(!$(n).data('mutate-top'))$(n).data('mutate-top', $(n).css('top'));
					
					if ($(n).data('mutate-top')&&$(n).css('top') != $(n).data('mutate-top')  ) {
						$(n).data('mutate-top', $(n).css('top'));
						return true;
					}
				}
			},
			{
				name		: 'bottom',
				handler 	: function (n){
					if(!$(n).data('mutate-bottom'))$(n).data('mutate-bottom', $(n).css('bottom'));
					
					if ($(n).data('mutate-bottom')&&$(n).css('bottom') != $(n).data('mutate-bottom')  ) {
						$(n).data('mutate-bottom', $(n).css('bottom'));
						return true;
					}
				}
			},
			{
				name		: 'right',
				handler 	: function (n){
					if(!$(n).data('mutate-right'))$(n).data('mutate-right', $(n).css('right'));
					
					if ($(n).data('mutate-right')&&$(n).css('right') != $(n).data('mutate-right')  ) {
						$(n).data('mutate-right', $(n).css('right'));
						return true;
					}
				}
			},
			{
				name		: 'left',
				handler 	: function (n){
					if(!$(n).data('mutate-left'))$(n).data('mutate-left', $(n).css('left'));
					
					if ($(n).data('mutate-left')&&$(n).css('left') != $(n).data('mutate-left')  ) {
						$(n).data('mutate-left', $(n).css('left'));
						return true;
					}
				}
			},
			{
				name		: 'hide',
				handler 	: function (n){ if ($(n).is(':hidden'))	return true; }
			},
			{
				name		: 'show',
				handler 	: function (n){ if ($(n).is(':visible'))	return true; }
			},
			{
				name		: 'scrollHeight',
				handler 	: function (n){
					if(!$(n).data('prev-scrollHeight'))$(n).data('prev-scrollHeight', $(n)[0].scrollHeight);
					
					if ($(n).data('prev-scrollHeight')&&$(n)[0].scrollHeight != $(n).data('prev-scrollHeight')  ) {
						$(n).data('prev-scrollHeight', $(n)[0].scrollHeight);
						return true;
					}
				}
			},
			{
				name		: 'scrollWidth',
				handler 	: function (n){
					if(!$(n).data('prev-scrollWidth'))$(n).data('prev-scrollWidth', $(n)[0].scrollWidth);
					
					if ($(n).data('prev-scrollWidth')&&$(n)[0].scrollWidth != $(n).data('prev-scrollWidth')  ) {
						$(n).data('prev-scrollWidth', $(n)[0].scrollWidth);
						return true;
					}
				}
			},
			{
				name		: 'scrollTop',
				handler 	: function (n){
					if(!$(n).data('prev-scrollTop'))$(n).data('prev-scrollTop', $(n)[0].scrollTop());
					
					if ($(n).data('prev-scrollTop')&&$(n)[0].scrollTop() != $(n).data('prev-scrollTop')  ) {
						$(n).data('prev-scrollTop', $(n)[0].scrollTop());
						return true;
					}
				}
			},
			{
				name		: 'scrollLeft',
				handler 	: function (n){
					if(!$(n).data('prev-scrollLeft'))$(n).data('prev-scrollLeft', $(n)[0].scrollLeft());
					
					if ($(n).data('prev-scrollLeft')&&$(n)[0].scrollLeft() != $(n).data('prev-scrollLeft')  ) {
						$(n).data('prev-scrollLeft', $(n)[0].scrollLeft());
						return true;
					}
				}
			}
		];
;(function($){mutate={speed:1,event_stack:mutate_event_stack,stack:[],events:{},add_event:function(evt){mutate.events[evt.name]=evt.handler;},add:function(event_name,selector,callback,false_callback){mutate.stack[mutate.stack.length]={event_name:event_name,selector:selector,callback:callback,false_callback:false_callback}}};function reset(){var parent=mutate;if(parent.event_stack!='undefined'&&parent.event_stack.length){$.each(parent.event_stack,function(j,k){mutate.add_event(k);});}
parent.event_stack=[];$.each(parent.stack,function(i,n){$(n.selector).each(function(a,b){if(parent.events[n.event_name](b)===true){if(n['callback'])n.callback(b,n);}else{if(n['false_callback'])n.false_callback(b,n)}})})
setTimeout(reset,mutate.speed);}
reset();$.fn.extend({mutate:function(){var event_name=false,callback=arguments[1],selector=this,false_callback=arguments[2]?arguments[2]:function(){};if(arguments[0].toLowerCase()=='extend'){mutate.add_event(callback);return this;}
$.each($.trim(arguments[0]).split(' '),function(i,n){event_name=n;mutate.add(event_name,selector,callback,false_callback);});return this;}});})(jQuery);
/*
 *	jQuery dotdotdot 1.6.16
 *
 *	Copyright (c) Fred Heusschen
 *	www.frebsite.nl
 *
 *	Plugin website:
 *	dotdotdot.frebsite.nl
 *
 *	Dual licensed under the MIT and GPL licenses.
 *	http://en.wikipedia.org/wiki/MIT_License
 *	http://en.wikipedia.org/wiki/GNU_General_Public_License
 */
!function(t,e){function n(t,e,n){var r=t.children(),o=!1;t.empty();for(var i=0,d=r.length;d>i;i++){var l=r.eq(i);if(t.append(l),n&&t.append(n),a(t,e)){l.remove(),o=!0;break}n&&n.detach()}return o}function r(e,n,i,d,l){var s=!1,c="table, thead, tbody, tfoot, tr, col, colgroup, object, embed, param, ol, ul, dl, blockquote, select, optgroup, option, textarea, script, style",u="script, .dotdotdot-keep";return e.contents().detach().each(function(){var f=this,h=t(f);if("undefined"==typeof f||3==f.nodeType&&0==t.trim(f.data).length)return!0;if(h.is(u))e.append(h);else{if(s)return!0;e.append(h),l&&e[e.is(c)?"after":"append"](l),a(i,d)&&(s=3==f.nodeType?o(h,n,i,d,l):r(h,n,i,d,l),s||(h.detach(),s=!0)),s||l&&l.detach()}}),s}function o(e,n,r,o,d){var c=e[0];if(!c)return!1;var f=s(c),h=-1!==f.indexOf(" ")?" ":"　",p="letter"==o.wrap?"":h,g=f.split(p),v=-1,w=-1,b=0,y=g.length-1;for(o.fallbackToLetter&&0==b&&0==y&&(p="",g=f.split(p),y=g.length-1);y>=b&&(0!=b||0!=y);){var m=Math.floor((b+y)/2);if(m==w)break;w=m,l(c,g.slice(0,w+1).join(p)+o.ellipsis),a(r,o)?(y=w,o.fallbackToLetter&&0==b&&0==y&&(p="",g=g[0].split(p),v=-1,w=-1,b=0,y=g.length-1)):(v=w,b=w)}if(-1==v||1==g.length&&0==g[0].length){var x=e.parent();e.detach();var T=d&&d.closest(x).length?d.length:0;x.contents().length>T?c=u(x.contents().eq(-1-T),n):(c=u(x,n,!0),T||x.detach()),c&&(f=i(s(c),o),l(c,f),T&&d&&t(c).parent().append(d))}else f=i(g.slice(0,v+1).join(p),o),l(c,f);return!0}function a(t,e){return t.innerHeight()>e.maxHeight}function i(e,n){for(;t.inArray(e.slice(-1),n.lastCharacter.remove)>-1;)e=e.slice(0,-1);return t.inArray(e.slice(-1),n.lastCharacter.noEllipsis)<0&&(e+=n.ellipsis),e}function d(t){return{width:t.innerWidth(),height:t.innerHeight()}}function l(t,e){t.innerText?t.innerText=e:t.nodeValue?t.nodeValue=e:t.textContent&&(t.textContent=e)}function s(t){return t.innerText?t.innerText:t.nodeValue?t.nodeValue:t.textContent?t.textContent:""}function c(t){do t=t.previousSibling;while(t&&1!==t.nodeType&&3!==t.nodeType);return t}function u(e,n,r){var o,a=e&&e[0];if(a){if(!r){if(3===a.nodeType)return a;if(t.trim(e.text()))return u(e.contents().last(),n)}for(o=c(a);!o;){if(e=e.parent(),e.is(n)||!e.length)return!1;o=c(e[0])}if(o)return u(t(o),n)}return!1}function f(e,n){return e?"string"==typeof e?(e=t(e,n),e.length?e:!1):e.jquery?e:!1:!1}function h(t){for(var e=t.innerHeight(),n=["paddingTop","paddingBottom"],r=0,o=n.length;o>r;r++){var a=parseInt(t.css(n[r]),10);isNaN(a)&&(a=0),e-=a}return e}if(!t.fn.dotdotdot){t.fn.dotdotdot=function(e){if(0==this.length)return t.fn.dotdotdot.debug('No element found for "'+this.selector+'".'),this;if(this.length>1)return this.each(function(){t(this).dotdotdot(e)});var o=this;o.data("dotdotdot")&&o.trigger("destroy.dot"),o.data("dotdotdot-style",o.attr("style")||""),o.css("word-wrap","break-word"),"nowrap"===o.css("white-space")&&o.css("white-space","normal"),o.bind_events=function(){return o.bind("update.dot",function(e,d){e.preventDefault(),e.stopPropagation(),l.maxHeight="number"==typeof l.height?l.height:h(o),l.maxHeight+=l.tolerance,"undefined"!=typeof d&&(("string"==typeof d||d instanceof HTMLElement)&&(d=t("<div />").append(d).contents()),d instanceof t&&(i=d)),g=o.wrapInner('<div class="dotdotdot" />').children(),g.contents().detach().end().append(i.clone(!0)).find("br").replaceWith("  <br />  ").end().css({height:"auto",width:"auto",border:"none",padding:0,margin:0});var c=!1,u=!1;return s.afterElement&&(c=s.afterElement.clone(!0),c.show(),s.afterElement.detach()),a(g,l)&&(u="children"==l.wrap?n(g,l,c):r(g,o,g,l,c)),g.replaceWith(g.contents()),g=null,t.isFunction(l.callback)&&l.callback.call(o[0],u,i),s.isTruncated=u,u}).bind("isTruncated.dot",function(t,e){return t.preventDefault(),t.stopPropagation(),"function"==typeof e&&e.call(o[0],s.isTruncated),s.isTruncated}).bind("originalContent.dot",function(t,e){return t.preventDefault(),t.stopPropagation(),"function"==typeof e&&e.call(o[0],i),i}).bind("destroy.dot",function(t){t.preventDefault(),t.stopPropagation(),o.unwatch().unbind_events().contents().detach().end().append(i).attr("style",o.data("dotdotdot-style")||"").data("dotdotdot",!1)}),o},o.unbind_events=function(){return o.unbind(".dot"),o},o.watch=function(){if(o.unwatch(),"window"==l.watch){var e=t(window),n=e.width(),r=e.height();e.bind("resize.dot"+s.dotId,function(){n==e.width()&&r==e.height()&&l.windowResizeFix||(n=e.width(),r=e.height(),u&&clearInterval(u),u=setTimeout(function(){o.trigger("update.dot")},100))})}else c=d(o),u=setInterval(function(){if(o.is(":visible")){var t=d(o);(c.width!=t.width||c.height!=t.height)&&(o.trigger("update.dot"),c=t)}},500);return o},o.unwatch=function(){return t(window).unbind("resize.dot"+s.dotId),u&&clearInterval(u),o};var i=o.contents(),l=t.extend(!0,{},t.fn.dotdotdot.defaults,e),s={},c={},u=null,g=null;return l.lastCharacter.remove instanceof Array||(l.lastCharacter.remove=t.fn.dotdotdot.defaultArrays.lastCharacter.remove),l.lastCharacter.noEllipsis instanceof Array||(l.lastCharacter.noEllipsis=t.fn.dotdotdot.defaultArrays.lastCharacter.noEllipsis),s.afterElement=f(l.after,o),s.isTruncated=!1,s.dotId=p++,o.data("dotdotdot",!0).bind_events().trigger("update.dot"),l.watch&&o.watch(),o},t.fn.dotdotdot.defaults={ellipsis:"... ",wrap:"word",fallbackToLetter:!0,lastCharacter:{},tolerance:0,callback:null,after:null,height:null,watch:!1,windowResizeFix:!0},t.fn.dotdotdot.defaultArrays={lastCharacter:{remove:[" ","　",",",";",".","!","?"],noEllipsis:[]}},t.fn.dotdotdot.debug=function(){};var p=1,g=t.fn.html;t.fn.html=function(n){return n!=e&&!t.isFunction(n)&&this.data("dotdotdot")?this.trigger("update",[n]):g.apply(this,arguments)};var v=t.fn.text;t.fn.text=function(n){return n!=e&&!t.isFunction(n)&&this.data("dotdotdot")?(n=t("<div />").text(n).html(),this.trigger("update",[n])):v.apply(this,arguments)}}}(jQuery);
/*! perfect-scrollbar - v0.4.11
* http://noraesae.github.com/perfect-scrollbar/
* Copyright (c) 2014 Hyeonje Alex Jun; Licensed MIT */
(function(e){"use strict";"function"==typeof define&&define.amd?define(["jquery"],e):"object"==typeof exports?e(require("jquery")):e(jQuery)})(function(e){"use strict";var t={wheelSpeed:10,wheelPropagation:!1,minScrollbarLength:null,maxScrollbarLength:null,useBothWheelAxes:!1,useKeyboard:!0,suppressScrollX:!1,suppressScrollY:!1,scrollXMarginOffset:0,scrollYMarginOffset:0,includePadding:!1},o=function(){var e=0;return function(){var t=e;return e+=1,".perfect-scrollbar-"+t}}();e.fn.perfectScrollbar=function(n,r){return this.each(function(){var l=e.extend(!0,{},t),s=e(this);if("object"==typeof n?e.extend(!0,l,n):r=n,"update"===r)return s.data("perfect-scrollbar-update")&&s.data("perfect-scrollbar-update")(),s;if("destroy"===r)return s.data("perfect-scrollbar-destroy")&&s.data("perfect-scrollbar-destroy")(),s;if(s.data("perfect-scrollbar"))return s.data("perfect-scrollbar");s.addClass("ps-container");var a,i,c,u,d,p,f,h,v,b,g=e("<div class='ps-scrollbar-x-rail'></div>").appendTo(s),m=e("<div class='ps-scrollbar-y-rail'></div>").appendTo(s),w=e("<div class='ps-scrollbar-x'></div>").appendTo(g),L=e("<div class='ps-scrollbar-y'></div>").appendTo(m),T=parseInt(g.css("bottom"),10),y=T===T,x=y?null:parseInt(g.css("top"),10),S=parseInt(m.css("right"),10),I=S===S,P=I?null:parseInt(m.css("left"),10),D="rtl"===s.css("direction"),M=o(),C=parseInt(g.css("borderLeftWidth"),10)+parseInt(g.css("borderRightWidth"),10),X=parseInt(g.css("borderTopWidth"),10)+parseInt(g.css("borderBottomWidth"),10),Y=function(e,t){var o=e+t,n=u-v;b=0>o?0:o>n?n:o;var r=parseInt(b*(p-u)/(u-v),10);s.scrollTop(r),y?g.css({bottom:T-r}):g.css({top:x+r})},k=function(e,t){var o=e+t,n=c-f;h=0>o?0:o>n?n:o;var r=parseInt(h*(d-c)/(c-f),10);s.scrollLeft(r),I?m.css({right:S-r}):m.css({left:P+r})},W=function(e){return l.minScrollbarLength&&(e=Math.max(e,l.minScrollbarLength)),l.maxScrollbarLength&&(e=Math.min(e,l.maxScrollbarLength)),e},j=function(){var e={width:c,display:a?"inherit":"none"};e.left=D?s.scrollLeft()+c-d:s.scrollLeft(),y?e.bottom=T-s.scrollTop():e.top=x+s.scrollTop(),g.css(e);var t={top:s.scrollTop(),height:u,display:i?"inherit":"none"};I?t.right=D?d-s.scrollLeft()-S-L.outerWidth():S-s.scrollLeft():t.left=D?s.scrollLeft()+2*c-d-P-L.outerWidth():P+s.scrollLeft(),m.css(t),w.css({left:h,width:f-C}),L.css({top:b,height:v-X}),a?s.addClass("ps-active-x"):s.removeClass("ps-active-x"),i?s.addClass("ps-active-y"):s.removeClass("ps-active-y")},O=function(){c=l.includePadding?s.innerWidth():s.width(),u=l.includePadding?s.innerHeight():s.height(),d=s.prop("scrollWidth"),p=s.prop("scrollHeight"),!l.suppressScrollX&&d>c+l.scrollXMarginOffset?(a=!0,f=W(parseInt(c*c/d,10)),h=parseInt(s.scrollLeft()*(c-f)/(d-c),10)):(a=!1,f=0,h=0,s.scrollLeft(0)),!l.suppressScrollY&&p>u+l.scrollYMarginOffset?(i=!0,v=W(parseInt(u*u/p,10)),b=parseInt(s.scrollTop()*(u-v)/(p-u),10)):(i=!1,v=0,b=0,s.scrollTop(0)),b>=u-v&&(b=u-v),h>=c-f&&(h=c-f),j()},E=function(){var t,o;w.bind("mousedown"+M,function(e){o=e.pageX,t=w.position().left,g.addClass("in-scrolling"),e.stopPropagation(),e.preventDefault()}),e(document).bind("mousemove"+M,function(e){g.hasClass("in-scrolling")&&(k(t,e.pageX-o),e.stopPropagation(),e.preventDefault())}),e(document).bind("mouseup"+M,function(){g.hasClass("in-scrolling")&&g.removeClass("in-scrolling")}),t=o=null},H=function(){var t,o;L.bind("mousedown"+M,function(e){o=e.pageY,t=L.position().top,m.addClass("in-scrolling"),e.stopPropagation(),e.preventDefault()}),e(document).bind("mousemove"+M,function(e){m.hasClass("in-scrolling")&&(Y(t,e.pageY-o),e.stopPropagation(),e.preventDefault())}),e(document).bind("mouseup"+M,function(){m.hasClass("in-scrolling")&&m.removeClass("in-scrolling")}),t=o=null},A=function(e,t){var o=s.scrollTop();if(0===e){if(!i)return!1;if(0===o&&t>0||o>=p-u&&0>t)return!l.wheelPropagation}var n=s.scrollLeft();if(0===t){if(!a)return!1;if(0===n&&0>e||n>=d-c&&e>0)return!l.wheelPropagation}return!0},q=function(){l.wheelSpeed/=10;var e=!1;s.bind("mousewheel"+M,function(t,o,n,r){var c=t.deltaX*t.deltaFactor||n,u=t.deltaY*t.deltaFactor||r;e=!1,l.useBothWheelAxes?i&&!a?(u?s.scrollTop(s.scrollTop()-u*l.wheelSpeed):s.scrollTop(s.scrollTop()+c*l.wheelSpeed),e=!0):a&&!i&&(c?s.scrollLeft(s.scrollLeft()+c*l.wheelSpeed):s.scrollLeft(s.scrollLeft()-u*l.wheelSpeed),e=!0):(s.scrollTop(s.scrollTop()-u*l.wheelSpeed),s.scrollLeft(s.scrollLeft()+c*l.wheelSpeed)),O(),e=e||A(c,u),e&&(t.stopPropagation(),t.preventDefault())}),s.bind("MozMousePixelScroll"+M,function(t){e&&t.preventDefault()})},B=function(){var t=!1;s.bind("mouseenter"+M,function(){t=!0}),s.bind("mouseleave"+M,function(){t=!1});var o=!1;e(document).bind("keydown"+M,function(n){if(!(n.isDefaultPrevented&&n.isDefaultPrevented()||!t||e(document.activeElement).is(":input,[contenteditable]"))){var r=0,l=0;switch(n.which){case 37:r=-30;break;case 38:l=30;break;case 39:r=30;break;case 40:l=-30;break;case 33:l=90;break;case 32:case 34:l=-90;break;case 35:l=-u;break;case 36:l=u;break;default:return}s.scrollTop(s.scrollTop()-l),s.scrollLeft(s.scrollLeft()+r),o=A(r,l),o&&n.preventDefault()}})},F=function(){var e=function(e){e.stopPropagation()};L.bind("click"+M,e),m.bind("click"+M,function(e){var t=parseInt(v/2,10),o=e.pageY-m.offset().top-t,n=u-v,r=o/n;0>r?r=0:r>1&&(r=1),s.scrollTop((p-u)*r)}),w.bind("click"+M,e),g.bind("click"+M,function(e){var t=parseInt(f/2,10),o=e.pageX-g.offset().left-t,n=c-f,r=o/n;0>r?r=0:r>1&&(r=1),s.scrollLeft((d-c)*r)})},z=function(){var t=function(e,t){s.scrollTop(s.scrollTop()-t),s.scrollLeft(s.scrollLeft()-e),O()},o={},n=0,r={},l=null,a=!1;e(window).bind("touchstart"+M,function(){a=!0}),e(window).bind("touchend"+M,function(){a=!1}),s.bind("touchstart"+M,function(e){var t=e.originalEvent.targetTouches[0];o.pageX=t.pageX,o.pageY=t.pageY,n=(new Date).getTime(),null!==l&&clearInterval(l),e.stopPropagation()}),s.bind("touchmove"+M,function(e){if(!a&&1===e.originalEvent.targetTouches.length){var l=e.originalEvent.targetTouches[0],s={};s.pageX=l.pageX,s.pageY=l.pageY;var i=s.pageX-o.pageX,c=s.pageY-o.pageY;t(i,c),o=s;var u=(new Date).getTime(),d=u-n;d>0&&(r.x=i/d,r.y=c/d,n=u),e.preventDefault()}}),s.bind("touchend"+M,function(){clearInterval(l),l=setInterval(function(){return.01>Math.abs(r.x)&&.01>Math.abs(r.y)?(clearInterval(l),void 0):(t(30*r.x,30*r.y),r.x*=.8,r.y*=.8,void 0)},10)})},K=function(){s.bind("scroll"+M,function(){O()})},Q=function(){s.unbind(M),e(window).unbind(M),e(document).unbind(M),s.data("perfect-scrollbar",null),s.data("perfect-scrollbar-update",null),s.data("perfect-scrollbar-destroy",null),w.remove(),L.remove(),g.remove(),m.remove(),g=m=w=L=a=i=c=u=d=p=f=h=T=y=x=v=b=S=I=P=D=M=null},R=function(t){s.addClass("ie").addClass("ie"+t);var o=function(){var t=function(){e(this).addClass("hover")},o=function(){e(this).removeClass("hover")};s.bind("mouseenter"+M,t).bind("mouseleave"+M,o),g.bind("mouseenter"+M,t).bind("mouseleave"+M,o),m.bind("mouseenter"+M,t).bind("mouseleave"+M,o),w.bind("mouseenter"+M,t).bind("mouseleave"+M,o),L.bind("mouseenter"+M,t).bind("mouseleave"+M,o)},n=function(){j=function(){var e={left:h+s.scrollLeft(),width:f};y?e.bottom=T:e.top=x,w.css(e);var t={top:b+s.scrollTop(),height:v};I?t.right=S:t.left=P,L.css(t),w.hide().show(),L.hide().show()}};6===t&&(o(),n())},N="ontouchstart"in window||window.DocumentTouch&&document instanceof window.DocumentTouch,Z=function(){var e=navigator.userAgent.toLowerCase().match(/(msie) ([\w.]+)/);e&&"msie"===e[1]&&R(parseInt(e[2],10)),O(),K(),E(),H(),F(),N&&z(),s.mousewheel&&q(),l.useKeyboard&&B(),s.data("perfect-scrollbar",s),s.data("perfect-scrollbar-update",O),s.data("perfect-scrollbar-destroy",Q)};return Z(),s})}}),function(e){"function"==typeof define&&define.amd?define(["jquery"],e):"object"==typeof exports?module.exports=e:e(jQuery)}(function(e){function t(t){var s=t||window.event,a=i.call(arguments,1),c=0,u=0,d=0,p=0;if(t=e.event.fix(s),t.type="mousewheel","detail"in s&&(d=-1*s.detail),"wheelDelta"in s&&(d=s.wheelDelta),"wheelDeltaY"in s&&(d=s.wheelDeltaY),"wheelDeltaX"in s&&(u=-1*s.wheelDeltaX),"axis"in s&&s.axis===s.HORIZONTAL_AXIS&&(u=-1*d,d=0),c=0===d?u:d,"deltaY"in s&&(d=-1*s.deltaY,c=d),"deltaX"in s&&(u=s.deltaX,0===d&&(c=-1*u)),0!==d||0!==u){if(1===s.deltaMode){var f=e.data(this,"mousewheel-line-height");c*=f,d*=f,u*=f}else if(2===s.deltaMode){var h=e.data(this,"mousewheel-page-height");c*=h,d*=h,u*=h}return p=Math.max(Math.abs(d),Math.abs(u)),(!l||l>p)&&(l=p,n(s,p)&&(l/=40)),n(s,p)&&(c/=40,u/=40,d/=40),c=Math[c>=1?"floor":"ceil"](c/l),u=Math[u>=1?"floor":"ceil"](u/l),d=Math[d>=1?"floor":"ceil"](d/l),t.deltaX=u,t.deltaY=d,t.deltaFactor=l,t.deltaMode=0,a.unshift(t,c,u,d),r&&clearTimeout(r),r=setTimeout(o,200),(e.event.dispatch||e.event.handle).apply(this,a)}}function o(){l=null}function n(e,t){return u.settings.adjustOldDeltas&&"mousewheel"===e.type&&0===t%120}var r,l,s=["wheel","mousewheel","DOMMouseScroll","MozMousePixelScroll"],a="onwheel"in document||document.documentMode>=9?["wheel"]:["mousewheel","DomMouseScroll","MozMousePixelScroll"],i=Array.prototype.slice;if(e.event.fixHooks)for(var c=s.length;c;)e.event.fixHooks[s[--c]]=e.event.mouseHooks;var u=e.event.special.mousewheel={version:"3.1.9",setup:function(){if(this.addEventListener)for(var o=a.length;o;)this.addEventListener(a[--o],t,!1);else this.onmousewheel=t;e.data(this,"mousewheel-line-height",u.getLineHeight(this)),e.data(this,"mousewheel-page-height",u.getPageHeight(this))},teardown:function(){if(this.removeEventListener)for(var e=a.length;e;)this.removeEventListener(a[--e],t,!1);else this.onmousewheel=null},getLineHeight:function(t){return parseInt(e(t)["offsetParent"in e.fn?"offsetParent":"parent"]().css("fontSize"),10)},getPageHeight:function(t){return e(t).height()},settings:{adjustOldDeltas:!0}};e.fn.extend({mousewheel:function(e){return e?this.bind("mousewheel",e):this.trigger("mousewheel")},unmousewheel:function(e){return this.unbind("mousewheel",e)}})});
/*
 * jQuery Easing v1.3 - http://gsgd.co.uk/sandbox/jquery/easing/
 *
 * Uses the built in easing capabilities added In jQuery 1.1
 * to offer multiple easing options
 *
 * TERMS OF USE - jQuery Easing
 * 
 * Open source under the BSD License. 
 * 
 * Copyright © 2008 George McGinley Smith
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of 
 * conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list 
 * of conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 * 
 * Neither the name of the author nor the names of contributors may be used to endorse 
 * or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 *  COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 *  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 *  GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED 
 * AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 *  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
 * OF THE POSSIBILITY OF SUCH DAMAGE. 
 *
*/

// t: current time, b: begInnIng value, c: change In value, d: duration
jQuery.easing['jswing'] = jQuery.easing['swing'];

jQuery.extend( jQuery.easing,
{
	def: 'easeOutQuad',
	swing: function (x, t, b, c, d) {
		//alert(jQuery.easing.default);
		return jQuery.easing[jQuery.easing.def](x, t, b, c, d);
	},
	easeInQuad: function (x, t, b, c, d) {
		return c*(t/=d)*t + b;
	},
	easeOutQuad: function (x, t, b, c, d) {
		return -c *(t/=d)*(t-2) + b;
	},
	easeInOutQuad: function (x, t, b, c, d) {
		if ((t/=d/2) < 1) return c/2*t*t + b;
		return -c/2 * ((--t)*(t-2) - 1) + b;
	},
	easeInCubic: function (x, t, b, c, d) {
		return c*(t/=d)*t*t + b;
	},
	easeOutCubic: function (x, t, b, c, d) {
		return c*((t=t/d-1)*t*t + 1) + b;
	},
	easeInOutCubic: function (x, t, b, c, d) {
		if ((t/=d/2) < 1) return c/2*t*t*t + b;
		return c/2*((t-=2)*t*t + 2) + b;
	},
	easeInQuart: function (x, t, b, c, d) {
		return c*(t/=d)*t*t*t + b;
	},
	easeOutQuart: function (x, t, b, c, d) {
		return -c * ((t=t/d-1)*t*t*t - 1) + b;
	},
	easeInOutQuart: function (x, t, b, c, d) {
		if ((t/=d/2) < 1) return c/2*t*t*t*t + b;
		return -c/2 * ((t-=2)*t*t*t - 2) + b;
	},
	easeInQuint: function (x, t, b, c, d) {
		return c*(t/=d)*t*t*t*t + b;
	},
	easeOutQuint: function (x, t, b, c, d) {
		return c*((t=t/d-1)*t*t*t*t + 1) + b;
	},
	easeInOutQuint: function (x, t, b, c, d) {
		if ((t/=d/2) < 1) return c/2*t*t*t*t*t + b;
		return c/2*((t-=2)*t*t*t*t + 2) + b;
	},
	easeInSine: function (x, t, b, c, d) {
		return -c * Math.cos(t/d * (Math.PI/2)) + c + b;
	},
	easeOutSine: function (x, t, b, c, d) {
		return c * Math.sin(t/d * (Math.PI/2)) + b;
	},
	easeInOutSine: function (x, t, b, c, d) {
		return -c/2 * (Math.cos(Math.PI*t/d) - 1) + b;
	},
	easeInExpo: function (x, t, b, c, d) {
		return (t==0) ? b : c * Math.pow(2, 10 * (t/d - 1)) + b;
	},
	easeOutExpo: function (x, t, b, c, d) {
		return (t==d) ? b+c : c * (-Math.pow(2, -10 * t/d) + 1) + b;
	},
	easeInOutExpo: function (x, t, b, c, d) {
		if (t==0) return b;
		if (t==d) return b+c;
		if ((t/=d/2) < 1) return c/2 * Math.pow(2, 10 * (t - 1)) + b;
		return c/2 * (-Math.pow(2, -10 * --t) + 2) + b;
	},
	easeInCirc: function (x, t, b, c, d) {
		return -c * (Math.sqrt(1 - (t/=d)*t) - 1) + b;
	},
	easeOutCirc: function (x, t, b, c, d) {
		return c * Math.sqrt(1 - (t=t/d-1)*t) + b;
	},
	easeInOutCirc: function (x, t, b, c, d) {
		if ((t/=d/2) < 1) return -c/2 * (Math.sqrt(1 - t*t) - 1) + b;
		return c/2 * (Math.sqrt(1 - (t-=2)*t) + 1) + b;
	},
	easeInElastic: function (x, t, b, c, d) {
		var s=1.70158;var p=0;var a=c;
		if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3;
		if (a < Math.abs(c)) { a=c; var s=p/4; }
		else var s = p/(2*Math.PI) * Math.asin (c/a);
		return -(a*Math.pow(2,10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )) + b;
	},
	easeOutElastic: function (x, t, b, c, d) {
		var s=1.70158;var p=0;var a=c;
		if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3;
		if (a < Math.abs(c)) { a=c; var s=p/4; }
		else var s = p/(2*Math.PI) * Math.asin (c/a);
		return a*Math.pow(2,-10*t) * Math.sin( (t*d-s)*(2*Math.PI)/p ) + c + b;
	},
	easeInOutElastic: function (x, t, b, c, d) {
		var s=1.70158;var p=0;var a=c;
		if (t==0) return b;  if ((t/=d/2)==2) return b+c;  if (!p) p=d*(.3*1.5);
		if (a < Math.abs(c)) { a=c; var s=p/4; }
		else var s = p/(2*Math.PI) * Math.asin (c/a);
		if (t < 1) return -.5*(a*Math.pow(2,10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )) + b;
		return a*Math.pow(2,-10*(t-=1)) * Math.sin( (t*d-s)*(2*Math.PI)/p )*.5 + c + b;
	},
	easeInBack: function (x, t, b, c, d, s) {
		if (s == undefined) s = 1.70158;
		return c*(t/=d)*t*((s+1)*t - s) + b;
	},
	easeOutBack: function (x, t, b, c, d, s) {
		if (s == undefined) s = 1.70158;
		return c*((t=t/d-1)*t*((s+1)*t + s) + 1) + b;
	},
	easeInOutBack: function (x, t, b, c, d, s) {
		if (s == undefined) s = 1.70158; 
		if ((t/=d/2) < 1) return c/2*(t*t*(((s*=(1.525))+1)*t - s)) + b;
		return c/2*((t-=2)*t*(((s*=(1.525))+1)*t + s) + 2) + b;
	},
	easeInBounce: function (x, t, b, c, d) {
		return c - jQuery.easing.easeOutBounce (x, d-t, 0, c, d) + b;
	},
	easeOutBounce: function (x, t, b, c, d) {
		if ((t/=d) < (1/2.75)) {
			return c*(7.5625*t*t) + b;
		} else if (t < (2/2.75)) {
			return c*(7.5625*(t-=(1.5/2.75))*t + .75) + b;
		} else if (t < (2.5/2.75)) {
			return c*(7.5625*(t-=(2.25/2.75))*t + .9375) + b;
		} else {
			return c*(7.5625*(t-=(2.625/2.75))*t + .984375) + b;
		}
	},
	easeInOutBounce: function (x, t, b, c, d) {
		if (t < d/2) return jQuery.easing.easeInBounce (x, t*2, 0, c, d) * .5 + b;
		return jQuery.easing.easeOutBounce (x, t*2-d, 0, c, d) * .5 + c*.5 + b;
	}
});

/*
 *
 * TERMS OF USE - EASING EQUATIONS
 * 
 * Open source under the BSD License. 
 * 
 * Copyright © 2001 Robert Penner
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 * 
 * Redistributions of source code must retain the above copyright notice, this list of 
 * conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list 
 * of conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 * 
 * Neither the name of the author nor the names of contributors may be used to endorse 
 * or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 *  COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 *  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 *  GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED 
 * AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 *  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED 
 * OF THE POSSIBILITY OF SUCH DAMAGE. 
 *
 */
/*!
 * jQuery toDictionary() plugin
 *
 * Version 1.2 (11 Apr 2011)
 *
 * Copyright (c) 2011 Robert Koritnik
 * Licensed under the terms of the MIT license
 * http://www.opensource.org/licenses/mit-license.php
 */
 
(function ($) {
 
    // #region String.prototype.formatForDictionary
    // add String prototype format function if it doesn't yet exist
    if ($.isFunction(String.prototype.formatForDictionary) === false)
    {
        String.prototype.formatForDictionary = function () {
            var s = this;
            var i = arguments.length;
            while (i--)
            {
                s = s.replace(new RegExp("\\{" + i + "\\}", "gim"), arguments[i]);
            }
            return s;
        };
    }
    // #endregion
 
    // #region Date.prototype.toISOString
    // add Date prototype toISOString function if it doesn't yet exist
    if ($.isFunction(Date.prototype.toISOString) === false)
    {
        Date.prototype.toISOString = function () {
            var pad = function (n, places) {
                n = n.toString();
                for (var i = n.length; i < places; i++)
                {
                    n = "0" + n;
                }
                return n;
            };
            var d = this;
            return "{0}-{1}-{2}T{3}:{4}:{5}.{6}Z".formatForDictionary(
                d.getUTCFullYear(),
                pad(d.getUTCMonth() + 1, 2),
                pad(d.getUTCDate(), 2),
                pad(d.getUTCHours(), 2),
                pad(d.getUTCMinutes(), 2),
                pad(d.getUTCSeconds(), 2),
                pad(d.getUTCMilliseconds(), 3)
            );
        };
    }
    // #endregion
 
    var _flatten = function (input, output, prefix, includeNulls) {
        if ($.isPlainObject(input))
        {
            for (var p in input)
            {
                if (includeNulls === true || typeof (input[p]) !== "undefined" && input[p] !== null)
                {
                    _flatten(input[p], output, prefix.length > 0 ? prefix + "." + p : p, includeNulls);
                }
            }
        }
        else
        {
            if ($.isArray(input))
            {
                $.each(input, function (index, value) {
                    _flatten(value, output, "{0}[{1}]".formatForDictionary(prefix, index));
                });
                return;
            }
            if (!$.isFunction(input))
            {
                if (input instanceof Date)
                {
                    output.push({ name: prefix, value: input.toISOString() });
                }
                else
                {
                    var val = typeof (input);
                    switch (val)
                    {
                        case "boolean":
                        case "number":
                            val = input;
                            break;
                        case "object":
                            // this property is null, because non-null objects are evaluated in first if branch
                            if (includeNulls !== true)
                            {
                                return;
                            }
                        default:
                            val = input || "";
                    }
                    output.push({ name: prefix, value: val });
                }
            }
        }
    };
 
    $.extend({
        toDictionary: function (data, prefix, includeNulls) {
            /// <summary>Flattens an arbitrary JSON object to a dictionary that Asp.net MVC default model binder understands.</summary>
            /// <param name="data" type="Object">Can either be a JSON object or a function that returns one.</data>
            /// <param name="prefix" type="String" Optional="true">Provide this parameter when you want the output names to be prefixed by something (ie. when flattening simple values).</param>
            /// <param name="includeNulls" type="Boolean" Optional="true">Set this to 'true' when you want null valued properties to be included in result (default is 'false').</param>
 
            // get data first if provided parameter is a function
            data = $.isFunction(data) ? data.call() : data;
 
            // is second argument "prefix" or "includeNulls"
            if (arguments.length === 2 && typeof (prefix) === "boolean")
            {
                includeNulls = prefix;
                prefix = "";
            }
 
            // set "includeNulls" default
            includeNulls = typeof (includeNulls) === "boolean" ? includeNulls : false;
 
            var result = [];
            _flatten(data, result, prefix || "", includeNulls);
 
            return result;
        }
    });
})(jQuery);
/*!
 * Select2 4.0.0
 * https://select2.github.io
 *
 * Released under the MIT license
 * https://github.com/select2/select2/blob/master/LICENSE.md
 */
(function (factory) {
  if (typeof define === 'function' && define.amd) {
    // AMD. Register as an anonymous module.
    define(['jquery'], factory);
  } else if (typeof exports === 'object') {
    // Node/CommonJS
    factory(require('jquery'));   
  } else {
    // Browser globals
    factory(jQuery);
  }
}(function (jQuery) {
  // This is needed so we can catch the AMD loader configuration and use it
  // The inner file should be wrapped (by `banner.start.js`) in a function that
  // returns the AMD loader references.
  var S2 =
(function () {
  // Restore the Select2 AMD loader so it can be used
  // Needed mostly in the language files, where the loader is not inserted
  if (jQuery && jQuery.fn && jQuery.fn.select2 && jQuery.fn.select2.amd) {
    var S2 = jQuery.fn.select2.amd;
  }
var S2;(function () { if (!S2 || !S2.requirejs) {
if (!S2) { S2 = {}; } else { require = S2; }
/**
 * @license almond 0.2.9 Copyright (c) 2011-2014, The Dojo Foundation All Rights Reserved.
 * Available via the MIT or new BSD license.
 * see: http://github.com/jrburke/almond for details
 */
//Going sloppy to avoid 'use strict' string cost, but strict practices should
//be followed.
/*jslint sloppy: true */
/*global setTimeout: false */

var requirejs, require, define;
(function (undef) {
    var main, req, makeMap, handlers,
        defined = {},
        waiting = {},
        config = {},
        defining = {},
        hasOwn = Object.prototype.hasOwnProperty,
        aps = [].slice,
        jsSuffixRegExp = /\.js$/;

    function hasProp(obj, prop) {
        return hasOwn.call(obj, prop);
    }

    /**
     * Given a relative module name, like ./something, normalize it to
     * a real name that can be mapped to a path.
     * @param {String} name the relative name
     * @param {String} baseName a real name that the name arg is relative
     * to.
     * @returns {String} normalized name
     */
    function normalize(name, baseName) {
        var nameParts, nameSegment, mapValue, foundMap, lastIndex,
            foundI, foundStarMap, starI, i, j, part,
            baseParts = baseName && baseName.split("/"),
            map = config.map,
            starMap = (map && map['*']) || {};

        //Adjust any relative paths.
        if (name && name.charAt(0) === ".") {
            //If have a base name, try to normalize against it,
            //otherwise, assume it is a top-level require that will
            //be relative to baseUrl in the end.
            if (baseName) {
                //Convert baseName to array, and lop off the last part,
                //so that . matches that "directory" and not name of the baseName's
                //module. For instance, baseName of "one/two/three", maps to
                //"one/two/three.js", but we want the directory, "one/two" for
                //this normalization.
                baseParts = baseParts.slice(0, baseParts.length - 1);
                name = name.split('/');
                lastIndex = name.length - 1;

                // Node .js allowance:
                if (config.nodeIdCompat && jsSuffixRegExp.test(name[lastIndex])) {
                    name[lastIndex] = name[lastIndex].replace(jsSuffixRegExp, '');
                }

                name = baseParts.concat(name);

                //start trimDots
                for (i = 0; i < name.length; i += 1) {
                    part = name[i];
                    if (part === ".") {
                        name.splice(i, 1);
                        i -= 1;
                    } else if (part === "..") {
                        if (i === 1 && (name[2] === '..' || name[0] === '..')) {
                            //End of the line. Keep at least one non-dot
                            //path segment at the front so it can be mapped
                            //correctly to disk. Otherwise, there is likely
                            //no path mapping for a path starting with '..'.
                            //This can still fail, but catches the most reasonable
                            //uses of ..
                            break;
                        } else if (i > 0) {
                            name.splice(i - 1, 2);
                            i -= 2;
                        }
                    }
                }
                //end trimDots

                name = name.join("/");
            } else if (name.indexOf('./') === 0) {
                // No baseName, so this is ID is resolved relative
                // to baseUrl, pull off the leading dot.
                name = name.substring(2);
            }
        }

        //Apply map config if available.
        if ((baseParts || starMap) && map) {
            nameParts = name.split('/');

            for (i = nameParts.length; i > 0; i -= 1) {
                nameSegment = nameParts.slice(0, i).join("/");

                if (baseParts) {
                    //Find the longest baseName segment match in the config.
                    //So, do joins on the biggest to smallest lengths of baseParts.
                    for (j = baseParts.length; j > 0; j -= 1) {
                        mapValue = map[baseParts.slice(0, j).join('/')];

                        //baseName segment has  config, find if it has one for
                        //this name.
                        if (mapValue) {
                            mapValue = mapValue[nameSegment];
                            if (mapValue) {
                                //Match, update name to the new value.
                                foundMap = mapValue;
                                foundI = i;
                                break;
                            }
                        }
                    }
                }

                if (foundMap) {
                    break;
                }

                //Check for a star map match, but just hold on to it,
                //if there is a shorter segment match later in a matching
                //config, then favor over this star map.
                if (!foundStarMap && starMap && starMap[nameSegment]) {
                    foundStarMap = starMap[nameSegment];
                    starI = i;
                }
            }

            if (!foundMap && foundStarMap) {
                foundMap = foundStarMap;
                foundI = starI;
            }

            if (foundMap) {
                nameParts.splice(0, foundI, foundMap);
                name = nameParts.join('/');
            }
        }

        return name;
    }

    function makeRequire(relName, forceSync) {
        return function () {
            //A version of a require function that passes a moduleName
            //value for items that may need to
            //look up paths relative to the moduleName
            return req.apply(undef, aps.call(arguments, 0).concat([relName, forceSync]));
        };
    }

    function makeNormalize(relName) {
        return function (name) {
            return normalize(name, relName);
        };
    }

    function makeLoad(depName) {
        return function (value) {
            defined[depName] = value;
        };
    }

    function callDep(name) {
        if (hasProp(waiting, name)) {
            var args = waiting[name];
            delete waiting[name];
            defining[name] = true;
            main.apply(undef, args);
        }

        if (!hasProp(defined, name) && !hasProp(defining, name)) {
            throw new Error('No ' + name);
        }
        return defined[name];
    }

    //Turns a plugin!resource to [plugin, resource]
    //with the plugin being undefined if the name
    //did not have a plugin prefix.
    function splitPrefix(name) {
        var prefix,
            index = name ? name.indexOf('!') : -1;
        if (index > -1) {
            prefix = name.substring(0, index);
            name = name.substring(index + 1, name.length);
        }
        return [prefix, name];
    }

    /**
     * Makes a name map, normalizing the name, and using a plugin
     * for normalization if necessary. Grabs a ref to plugin
     * too, as an optimization.
     */
    makeMap = function (name, relName) {
        var plugin,
            parts = splitPrefix(name),
            prefix = parts[0];

        name = parts[1];

        if (prefix) {
            prefix = normalize(prefix, relName);
            plugin = callDep(prefix);
        }

        //Normalize according
        if (prefix) {
            if (plugin && plugin.normalize) {
                name = plugin.normalize(name, makeNormalize(relName));
            } else {
                name = normalize(name, relName);
            }
        } else {
            name = normalize(name, relName);
            parts = splitPrefix(name);
            prefix = parts[0];
            name = parts[1];
            if (prefix) {
                plugin = callDep(prefix);
            }
        }

        //Using ridiculous property names for space reasons
        return {
            f: prefix ? prefix + '!' + name : name, //fullName
            n: name,
            pr: prefix,
            p: plugin
        };
    };

    function makeConfig(name) {
        return function () {
            return (config && config.config && config.config[name]) || {};
        };
    }

    handlers = {
        require: function (name) {
            return makeRequire(name);
        },
        exports: function (name) {
            var e = defined[name];
            if (typeof e !== 'undefined') {
                return e;
            } else {
                return (defined[name] = {});
            }
        },
        module: function (name) {
            return {
                id: name,
                uri: '',
                exports: defined[name],
                config: makeConfig(name)
            };
        }
    };

    main = function (name, deps, callback, relName) {
        var cjsModule, depName, ret, map, i,
            args = [],
            callbackType = typeof callback,
            usingExports;

        //Use name if no relName
        relName = relName || name;

        //Call the callback to define the module, if necessary.
        if (callbackType === 'undefined' || callbackType === 'function') {
            //Pull out the defined dependencies and pass the ordered
            //values to the callback.
            //Default to [require, exports, module] if no deps
            deps = !deps.length && callback.length ? ['require', 'exports', 'module'] : deps;
            for (i = 0; i < deps.length; i += 1) {
                map = makeMap(deps[i], relName);
                depName = map.f;

                //Fast path CommonJS standard dependencies.
                if (depName === "require") {
                    args[i] = handlers.require(name);
                } else if (depName === "exports") {
                    //CommonJS module spec 1.1
                    args[i] = handlers.exports(name);
                    usingExports = true;
                } else if (depName === "module") {
                    //CommonJS module spec 1.1
                    cjsModule = args[i] = handlers.module(name);
                } else if (hasProp(defined, depName) ||
                           hasProp(waiting, depName) ||
                           hasProp(defining, depName)) {
                    args[i] = callDep(depName);
                } else if (map.p) {
                    map.p.load(map.n, makeRequire(relName, true), makeLoad(depName), {});
                    args[i] = defined[depName];
                } else {
                    throw new Error(name + ' missing ' + depName);
                }
            }

            ret = callback ? callback.apply(defined[name], args) : undefined;

            if (name) {
                //If setting exports via "module" is in play,
                //favor that over return value and exports. After that,
                //favor a non-undefined return value over exports use.
                if (cjsModule && cjsModule.exports !== undef &&
                        cjsModule.exports !== defined[name]) {
                    defined[name] = cjsModule.exports;
                } else if (ret !== undef || !usingExports) {
                    //Use the return value from the function.
                    defined[name] = ret;
                }
            }
        } else if (name) {
            //May just be an object definition for the module. Only
            //worry about defining if have a module name.
            defined[name] = callback;
        }
    };

    requirejs = require = req = function (deps, callback, relName, forceSync, alt) {
        if (typeof deps === "string") {
            if (handlers[deps]) {
                //callback in this case is really relName
                return handlers[deps](callback);
            }
            //Just return the module wanted. In this scenario, the
            //deps arg is the module name, and second arg (if passed)
            //is just the relName.
            //Normalize module name, if it contains . or ..
            return callDep(makeMap(deps, callback).f);
        } else if (!deps.splice) {
            //deps is a config object, not an array.
            config = deps;
            if (config.deps) {
                req(config.deps, config.callback);
            }
            if (!callback) {
                return;
            }

            if (callback.splice) {
                //callback is an array, which means it is a dependency list.
                //Adjust args if there are dependencies
                deps = callback;
                callback = relName;
                relName = null;
            } else {
                deps = undef;
            }
        }

        //Support require(['a'])
        callback = callback || function () {};

        //If relName is a function, it is an errback handler,
        //so remove it.
        if (typeof relName === 'function') {
            relName = forceSync;
            forceSync = alt;
        }

        //Simulate async callback;
        if (forceSync) {
            main(undef, deps, callback, relName);
        } else {
            //Using a non-zero value because of concern for what old browsers
            //do, and latest browsers "upgrade" to 4 if lower value is used:
            //http://www.whatwg.org/specs/web-apps/current-work/multipage/timers.html#dom-windowtimers-settimeout:
            //If want a value immediately, use require('id') instead -- something
            //that works in almond on the global level, but not guaranteed and
            //unlikely to work in other AMD implementations.
            setTimeout(function () {
                main(undef, deps, callback, relName);
            }, 4);
        }

        return req;
    };

    /**
     * Just drops the config on the floor, but returns req in case
     * the config return value is used.
     */
    req.config = function (cfg) {
        return req(cfg);
    };

    /**
     * Expose module registry for debugging and tooling
     */
    requirejs._defined = defined;

    define = function (name, deps, callback) {

        //This module may not have dependencies
        if (!deps.splice) {
            //deps is not an array, so probably means
            //an object literal or factory function for
            //the value. Adjust args.
            callback = deps;
            deps = [];
        }

        if (!hasProp(defined, name) && !hasProp(waiting, name)) {
            waiting[name] = [name, deps, callback];
        }
    };

    define.amd = {
        jQuery: true
    };
}());

S2.requirejs = requirejs;S2.require = require;S2.define = define;
}
}());
S2.define("almond", function(){});

/* global jQuery:false, $:false */
S2.define('jquery',[],function () {
  var _$ = jQuery || $;

  if (_$ == null && console && console.error) {
    console.error(
      'Select2: An instance of jQuery or a jQuery-compatible library was not ' +
      'found. Make sure that you are including jQuery before Select2 on your ' +
      'web page.'
    );
  }

  return _$;
});

S2.define('select2/utils',[
  'jquery'
], function ($) {
  var Utils = {};

  Utils.Extend = function (ChildClass, SuperClass) {
    var __hasProp = {}.hasOwnProperty;

    function BaseConstructor () {
      this.constructor = ChildClass;
    }

    for (var key in SuperClass) {
      if (__hasProp.call(SuperClass, key)) {
        ChildClass[key] = SuperClass[key];
      }
    }

    BaseConstructor.prototype = SuperClass.prototype;
    ChildClass.prototype = new BaseConstructor();
    ChildClass.__super__ = SuperClass.prototype;

    return ChildClass;
  };

  function getMethods (theClass) {
    var proto = theClass.prototype;

    var methods = [];

    for (var methodName in proto) {
      var m = proto[methodName];

      if (typeof m !== 'function') {
        continue;
      }

      if (methodName === 'constructor') {
        continue;
      }

      methods.push(methodName);
    }

    return methods;
  }

  Utils.Decorate = function (SuperClass, DecoratorClass) {
    var decoratedMethods = getMethods(DecoratorClass);
    var superMethods = getMethods(SuperClass);

    function DecoratedClass () {
      var unshift = Array.prototype.unshift;

      var argCount = DecoratorClass.prototype.constructor.length;

      var calledConstructor = SuperClass.prototype.constructor;

      if (argCount > 0) {
        unshift.call(arguments, SuperClass.prototype.constructor);

        calledConstructor = DecoratorClass.prototype.constructor;
      }

      calledConstructor.apply(this, arguments);
    }

    DecoratorClass.displayName = SuperClass.displayName;

    function ctr () {
      this.constructor = DecoratedClass;
    }

    DecoratedClass.prototype = new ctr();

    for (var m = 0; m < superMethods.length; m++) {
        var superMethod = superMethods[m];

        DecoratedClass.prototype[superMethod] =
          SuperClass.prototype[superMethod];
    }

    var calledMethod = function (methodName) {
      // Stub out the original method if it's not decorating an actual method
      var originalMethod = function () {};

      if (methodName in DecoratedClass.prototype) {
        originalMethod = DecoratedClass.prototype[methodName];
      }

      var decoratedMethod = DecoratorClass.prototype[methodName];

      return function () {
        var unshift = Array.prototype.unshift;

        unshift.call(arguments, originalMethod);

        return decoratedMethod.apply(this, arguments);
      };
    };

    for (var d = 0; d < decoratedMethods.length; d++) {
      var decoratedMethod = decoratedMethods[d];

      DecoratedClass.prototype[decoratedMethod] = calledMethod(decoratedMethod);
    }

    return DecoratedClass;
  };

  var Observable = function () {
    this.listeners = {};
  };

  Observable.prototype.on = function (event, callback) {
    this.listeners = this.listeners || {};

    if (event in this.listeners) {
      this.listeners[event].push(callback);
    } else {
      this.listeners[event] = [callback];
    }
  };

  Observable.prototype.trigger = function (event) {
    var slice = Array.prototype.slice;

    this.listeners = this.listeners || {};

    if (event in this.listeners) {
      this.invoke(this.listeners[event], slice.call(arguments, 1));
    }

    if ('*' in this.listeners) {
      this.invoke(this.listeners['*'], arguments);
    }
  };

  Observable.prototype.invoke = function (listeners, params) {
    for (var i = 0, len = listeners.length; i < len; i++) {
      listeners[i].apply(this, params);
    }
  };

  Utils.Observable = Observable;

  Utils.generateChars = function (length) {
    var chars = '';

    for (var i = 0; i < length; i++) {
      var randomChar = Math.floor(Math.random() * 36);
      chars += randomChar.toString(36);
    }

    return chars;
  };

  Utils.bind = function (func, context) {
    return function () {
      func.apply(context, arguments);
    };
  };

  Utils._convertData = function (data) {
    for (var originalKey in data) {
      var keys = originalKey.split('-');

      var dataLevel = data;

      if (keys.length === 1) {
        continue;
      }

      for (var k = 0; k < keys.length; k++) {
        var key = keys[k];

        // Lowercase the first letter
        // By default, dash-separated becomes camelCase
        key = key.substring(0, 1).toLowerCase() + key.substring(1);

        if (!(key in dataLevel)) {
          dataLevel[key] = {};
        }

        if (k == keys.length - 1) {
          dataLevel[key] = data[originalKey];
        }

        dataLevel = dataLevel[key];
      }

      delete data[originalKey];
    }

    return data;
  };

  Utils.hasScroll = function (index, el) {
    // Adapted from the function created by @ShadowScripter
    // and adapted by @BillBarry on the Stack Exchange Code Review website.
    // The original code can be found at
    // http://codereview.stackexchange.com/q/13338
    // and was designed to be used with the Sizzle selector engine.

    var $el = $(el);
    var overflowX = el.style.overflowX;
    var overflowY = el.style.overflowY;

    //Check both x and y declarations
    if (overflowX === overflowY &&
        (overflowY === 'hidden' || overflowY === 'visible')) {
      return false;
    }

    if (overflowX === 'scroll' || overflowY === 'scroll') {
      return true;
    }

    return ($el.innerHeight() < el.scrollHeight ||
      $el.innerWidth() < el.scrollWidth);
  };

  Utils.escapeMarkup = function (markup) {
    var replaceMap = {
      '\\': '&#92;',
      '&': '&amp;',
      '<': '&lt;',
      '>': '&gt;',
      '"': '&quot;',
      '\'': '&#39;',
      '/': '&#47;'
    };

    // Do not try to escape the markup if it's not a string
    if (typeof markup !== 'string') {
      return markup;
    }

    return String(markup).replace(/[&<>"'\/\\]/g, function (match) {
      return replaceMap[match];
    });
  };

  // Append an array of jQuery nodes to a given element.
  Utils.appendMany = function ($element, $nodes) {
    // jQuery 1.7.x does not support $.fn.append() with an array
    // Fall back to a jQuery object collection using $.fn.add()
    if ($.fn.jquery.substr(0, 3) === '1.7') {
      var $jqNodes = $();

      $.map($nodes, function (node) {
        $jqNodes = $jqNodes.add(node);
      });

      $nodes = $jqNodes;
    }

    $element.append($nodes);
  };

  return Utils;
});

S2.define('select2/results',[
  'jquery',
  './utils'
], function ($, Utils) {
  function Results ($element, options, dataAdapter) {
    this.$element = $element;
    this.data = dataAdapter;
    this.options = options;

    Results.__super__.constructor.call(this);
  }

  Utils.Extend(Results, Utils.Observable);

  Results.prototype.render = function () {
    var $results = $(
      '<ul class="select2-results__options" role="tree"></ul>'
    );

    if (this.options.get('multiple')) {
      $results.attr('aria-multiselectable', 'true');
    }

    this.$results = $results;

    return $results;
  };

  Results.prototype.clear = function () {
    this.$results.empty();
  };

  Results.prototype.displayMessage = function (params) {
    var escapeMarkup = this.options.get('escapeMarkup');

    this.clear();
    this.hideLoading();

    var $message = $(
      '<li role="treeitem" class="select2-results__option"></li>'
    );

    var message = this.options.get('translations').get(params.message);

    $message.append(
      escapeMarkup(
        message(params.args)
      )
    );

    this.$results.append($message);
  };

  Results.prototype.append = function (data) {
    this.hideLoading();

    var $options = [];

    if (data.results == null || data.results.length === 0) {
      if (this.$results.children().length === 0) {
        this.trigger('results:message', {
          message: 'noResults'
        });
      }

      return;
    }

    data.results = this.sort(data.results);

    for (var d = 0; d < data.results.length; d++) {
      var item = data.results[d];

      var $option = this.option(item);

      $options.push($option);
    }

    this.$results.append($options);
  };

  Results.prototype.position = function ($results, $dropdown) {
    var $resultsContainer = $dropdown.find('.select2-results');
    $resultsContainer.append($results);
  };

  Results.prototype.sort = function (data) {
    var sorter = this.options.get('sorter');

    return sorter(data);
  };

  Results.prototype.setClasses = function () {
    var self = this;

    this.data.current(function (selected) {
      var selectedIds = $.map(selected, function (s) {
        return s.id.toString();
      });

      var $options = self.$results
        .find('.select2-results__option[aria-selected]');

      $options.each(function () {
        var $option = $(this);

        var item = $.data(this, 'data');

        // id needs to be converted to a string when comparing
        var id = '' + item.id;

        if ((item.element != null && item.element.selected) ||
            (item.element == null && $.inArray(id, selectedIds) > -1)) {
          $option.attr('aria-selected', 'true');
        } else {
          $option.attr('aria-selected', 'false');
        }
      });

      var $selected = $options.filter('[aria-selected=true]');

      // Check if there are any selected options
      if ($selected.length > 0) {
        // If there are selected options, highlight the first
        $selected.first().trigger('mouseenter');
      } else {
        // If there are no selected options, highlight the first option
        // in the dropdown
        $options.first().trigger('mouseenter');
      }
    });
  };

  Results.prototype.showLoading = function (params) {
    this.hideLoading();

    var loadingMore = this.options.get('translations').get('searching');

    var loading = {
      disabled: true,
      loading: true,
      text: loadingMore(params)
    };
    var $loading = this.option(loading);
    $loading.className += ' loading-results';

    this.$results.prepend($loading);
  };

  Results.prototype.hideLoading = function () {
    this.$results.find('.loading-results').remove();
  };

  Results.prototype.option = function (data) {
    var option = document.createElement('li');
    option.className = 'select2-results__option';

    var attrs = {
      'role': 'treeitem',
      'aria-selected': 'false'
    };

    if (data.disabled) {
      delete attrs['aria-selected'];
      attrs['aria-disabled'] = 'true';
    }

    if (data.id == null) {
      delete attrs['aria-selected'];
    }

    if (data._resultId != null) {
      option.id = data._resultId;
    }

    if (data.title) {
      option.title = data.title;
    }

    if (data.children) {
      attrs.role = 'group';
      attrs['aria-label'] = data.text;
      delete attrs['aria-selected'];
    }

    for (var attr in attrs) {
      var val = attrs[attr];

      option.setAttribute(attr, val);
    }

    if (data.children) {
      var $option = $(option);

      var label = document.createElement('strong');
      label.className = 'select2-results__group';

      var $label = $(label);
      this.template(data, label);

      var $children = [];

      for (var c = 0; c < data.children.length; c++) {
        var child = data.children[c];

        var $child = this.option(child);

        $children.push($child);
      }

      var $childrenContainer = $('<ul></ul>', {
        'class': 'select2-results__options select2-results__options--nested'
      });

      $childrenContainer.append($children);

      $option.append(label);
      $option.append($childrenContainer);
    } else {
      this.template(data, option);
    }

    $.data(option, 'data', data);

    return option;
  };

  Results.prototype.bind = function (container, $container) {
    var self = this;

    var id = container.id + '-results';

    this.$results.attr('id', id);

    container.on('results:all', function (params) {
      self.clear();
      self.append(params.data);

      if (container.isOpen()) {
        self.setClasses();
      }
    });

    container.on('results:append', function (params) {
      self.append(params.data);

      if (container.isOpen()) {
        self.setClasses();
      }
    });

    container.on('query', function (params) {
      self.showLoading(params);
    });

    container.on('select', function () {
      if (!container.isOpen()) {
        return;
      }

      self.setClasses();
    });

    container.on('unselect', function () {
      if (!container.isOpen()) {
        return;
      }

      self.setClasses();
    });

    container.on('open', function () {
      // When the dropdown is open, aria-expended="true"
      self.$results.attr('aria-expanded', 'true');
      self.$results.attr('aria-hidden', 'false');

      self.setClasses();
      self.ensureHighlightVisible();
    });

    container.on('close', function () {
      // When the dropdown is closed, aria-expended="false"
      self.$results.attr('aria-expanded', 'false');
      self.$results.attr('aria-hidden', 'true');
      self.$results.removeAttr('aria-activedescendant');
    });

    container.on('results:toggle', function () {
      var $highlighted = self.getHighlightedResults();

      if ($highlighted.length === 0) {
        return;
      }

      $highlighted.trigger('mouseup');
    });

    container.on('results:select', function () {
      var $highlighted = self.getHighlightedResults();

      if ($highlighted.length === 0) {
        return;
      }

      var data = $highlighted.data('data');

      if ($highlighted.attr('aria-selected') == 'true') {
        self.trigger('close');
      } else {
        self.trigger('select', {
          data: data
        });
      }
    });

    container.on('results:previous', function () {
      var $highlighted = self.getHighlightedResults();

      var $options = self.$results.find('[aria-selected]');

      var currentIndex = $options.index($highlighted);

      // If we are already at te top, don't move further
      if (currentIndex === 0) {
        return;
      }

      var nextIndex = currentIndex - 1;

      // If none are highlighted, highlight the first
      if ($highlighted.length === 0) {
        nextIndex = 0;
      }

      var $next = $options.eq(nextIndex);

      $next.trigger('mouseenter');

      var currentOffset = self.$results.offset().top;
      var nextTop = $next.offset().top;
      var nextOffset = self.$results.scrollTop() + (nextTop - currentOffset);

      if (nextIndex === 0) {
        self.$results.scrollTop(0);
      } else if (nextTop - currentOffset < 0) {
        self.$results.scrollTop(nextOffset);
      }
    });

    container.on('results:next', function () {
      var $highlighted = self.getHighlightedResults();

      var $options = self.$results.find('[aria-selected]');

      var currentIndex = $options.index($highlighted);

      var nextIndex = currentIndex + 1;

      // If we are at the last option, stay there
      if (nextIndex >= $options.length) {
        return;
      }

      var $next = $options.eq(nextIndex);

      $next.trigger('mouseenter');

      var currentOffset = self.$results.offset().top +
        self.$results.outerHeight(false);
      var nextBottom = $next.offset().top + $next.outerHeight(false);
      var nextOffset = self.$results.scrollTop() + nextBottom - currentOffset;

      if (nextIndex === 0) {
        self.$results.scrollTop(0);
      } else if (nextBottom > currentOffset) {
        self.$results.scrollTop(nextOffset);
      }
    });

    container.on('results:focus', function (params) {
      params.element.addClass('select2-results__option--highlighted');
    });

    container.on('results:message', function (params) {
      self.displayMessage(params);
    });

    if ($.fn.mousewheel) {
      this.$results.on('mousewheel', function (e) {
        var top = self.$results.scrollTop();

        var bottom = (
          self.$results.get(0).scrollHeight -
          self.$results.scrollTop() +
          e.deltaY
        );

        var isAtTop = e.deltaY > 0 && top - e.deltaY <= 0;
        var isAtBottom = e.deltaY < 0 && bottom <= self.$results.height();

        if (isAtTop) {
          self.$results.scrollTop(0);

          e.preventDefault();
          e.stopPropagation();
        } else if (isAtBottom) {
          self.$results.scrollTop(
            self.$results.get(0).scrollHeight - self.$results.height()
          );

          e.preventDefault();
          e.stopPropagation();
        }
      });
    }

    this.$results.on('mouseup', '.select2-results__option[aria-selected]',
      function (evt) {
      var $this = $(this);

      var data = $this.data('data');

      if ($this.attr('aria-selected') === 'true') {
        if (self.options.get('multiple')) {
          self.trigger('unselect', {
            originalEvent: evt,
            data: data
          });
        } else {
          self.trigger('close');
        }

        return;
      }

      self.trigger('select', {
        originalEvent: evt,
        data: data
      });
    });

    this.$results.on('mouseenter', '.select2-results__option[aria-selected]',
      function (evt) {
      var data = $(this).data('data');

      self.getHighlightedResults()
          .removeClass('select2-results__option--highlighted');

      self.trigger('results:focus', {
        data: data,
        element: $(this)
      });
    });
  };

  Results.prototype.getHighlightedResults = function () {
    var $highlighted = this.$results
    .find('.select2-results__option--highlighted');

    return $highlighted;
  };

  Results.prototype.destroy = function () {
    this.$results.remove();
  };

  Results.prototype.ensureHighlightVisible = function () {
    var $highlighted = this.getHighlightedResults();

    if ($highlighted.length === 0) {
      return;
    }

    var $options = this.$results.find('[aria-selected]');

    var currentIndex = $options.index($highlighted);

    var currentOffset = this.$results.offset().top;
    var nextTop = $highlighted.offset().top;
    var nextOffset = this.$results.scrollTop() + (nextTop - currentOffset);

    var offsetDelta = nextTop - currentOffset;
    nextOffset -= $highlighted.outerHeight(false) * 2;

    if (currentIndex <= 2) {
      this.$results.scrollTop(0);
    } else if (offsetDelta > this.$results.outerHeight() || offsetDelta < 0) {
      this.$results.scrollTop(nextOffset);
    }
  };

  Results.prototype.template = function (result, container) {
    var template = this.options.get('templateResult');
    var escapeMarkup = this.options.get('escapeMarkup');

    var content = template(result);

    if (content == null) {
      container.style.display = 'none';
    } else if (typeof content === 'string') {
      container.innerHTML = escapeMarkup(content);
    } else {
      $(container).append(content);
    }
  };

  return Results;
});

S2.define('select2/keys',[

], function () {
  var KEYS = {
    BACKSPACE: 8,
    TAB: 9,
    ENTER: 13,
    SHIFT: 16,
    CTRL: 17,
    ALT: 18,
    ESC: 27,
    SPACE: 32,
    PAGE_UP: 33,
    PAGE_DOWN: 34,
    END: 35,
    HOME: 36,
    LEFT: 37,
    UP: 38,
    RIGHT: 39,
    DOWN: 40,
    DELETE: 46
  };

  return KEYS;
});

S2.define('select2/selection/base',[
  'jquery',
  '../utils',
  '../keys'
], function ($, Utils, KEYS) {
  function BaseSelection ($element, options) {
    this.$element = $element;
    this.options = options;

    BaseSelection.__super__.constructor.call(this);
  }

  Utils.Extend(BaseSelection, Utils.Observable);

  BaseSelection.prototype.render = function () {
    var $selection = $(
      '<span class="select2-selection" role="combobox" ' +
      'aria-autocomplete="list" aria-haspopup="true" aria-expanded="false">' +
      '</span>'
    );

    this._tabindex = 0;

    if (this.$element.data('old-tabindex') != null) {
      this._tabindex = this.$element.data('old-tabindex');
    } else if (this.$element.attr('tabindex') != null) {
      this._tabindex = this.$element.attr('tabindex');
    }

    $selection.attr('title', this.$element.attr('title'));
    $selection.attr('tabindex', this._tabindex);

    this.$selection = $selection;

    return $selection;
  };

  BaseSelection.prototype.bind = function (container, $container) {
    var self = this;

    var id = container.id + '-container';
    var resultsId = container.id + '-results';

    this.container = container;

    this.$selection.on('focus', function (evt) {
      self.trigger('focus', evt);
    });

    this.$selection.on('blur', function (evt) {
      self.trigger('blur', evt);
    });

    this.$selection.on('keydown', function (evt) {
      self.trigger('keypress', evt);

      if (evt.which === KEYS.SPACE) {
        evt.preventDefault();
      }
    });

    container.on('results:focus', function (params) {
      self.$selection.attr('aria-activedescendant', params.data._resultId);
    });

    container.on('selection:update', function (params) {
      self.update(params.data);
    });

    container.on('open', function () {
      // When the dropdown is open, aria-expanded="true"
      self.$selection.attr('aria-expanded', 'true');
      self.$selection.attr('aria-owns', resultsId);

      self._attachCloseHandler(container);
    });

    container.on('close', function () {
      // When the dropdown is closed, aria-expanded="false"
      self.$selection.attr('aria-expanded', 'false');
      self.$selection.removeAttr('aria-activedescendant');
      self.$selection.removeAttr('aria-owns');

      self.$selection.focus();

      self._detachCloseHandler(container);
    });

    container.on('enable', function () {
      self.$selection.attr('tabindex', self._tabindex);
    });

    container.on('disable', function () {
      self.$selection.attr('tabindex', '-1');
    });
  };

  BaseSelection.prototype._attachCloseHandler = function (container) {
    var self = this;

    $(document.body).on('mousedown.select2.' + container.id, function (e) {
      var $target = $(e.target);

      var $select = $target.closest('.select2');

      var $all = $('.select2.select2-container--open');

      $all.each(function () {
        var $this = $(this);

        if (this == $select[0]) {
          return;
        }

        var $element = $this.data('element');

        $element.select2('close');
      });
    });
  };

  BaseSelection.prototype._detachCloseHandler = function (container) {
    $(document.body).off('mousedown.select2.' + container.id);
  };

  BaseSelection.prototype.position = function ($selection, $container) {
    var $selectionContainer = $container.find('.selection');
    $selectionContainer.append($selection);
  };

  BaseSelection.prototype.destroy = function () {
    this._detachCloseHandler(this.container);
  };

  BaseSelection.prototype.update = function (data) {
    throw new Error('The `update` method must be defined in child classes.');
  };

  return BaseSelection;
});

S2.define('select2/selection/single',[
  'jquery',
  './base',
  '../utils',
  '../keys'
], function ($, BaseSelection, Utils, KEYS) {
  function SingleSelection () {
    SingleSelection.__super__.constructor.apply(this, arguments);
  }

  Utils.Extend(SingleSelection, BaseSelection);

  SingleSelection.prototype.render = function () {
    var $selection = SingleSelection.__super__.render.call(this);

    $selection.addClass('select2-selection--single');

    $selection.html(
      '<span class="select2-selection__rendered"></span>' +
      '<span class="select2-selection__arrow" role="presentation">' +
        '<b role="presentation"></b>' +
      '</span>'
    );

    return $selection;
  };

  SingleSelection.prototype.bind = function (container, $container) {
    var self = this;

    SingleSelection.__super__.bind.apply(this, arguments);

    var id = container.id + '-container';

    this.$selection.find('.select2-selection__rendered').attr('id', id);
    this.$selection.attr('aria-labelledby', id);

    this.$selection.on('mousedown', function (evt) {
      // Only respond to left clicks
      if (evt.which !== 1) {
        return;
      }

      self.trigger('toggle', {
        originalEvent: evt
      });
    });

    this.$selection.on('focus', function (evt) {
      // User focuses on the container
    });

    this.$selection.on('blur', function (evt) {
      // User exits the container
    });

    container.on('selection:update', function (params) {
      self.update(params.data);
    });
  };

  SingleSelection.prototype.clear = function () {
    this.$selection.find('.select2-selection__rendered').empty();
  };

  SingleSelection.prototype.display = function (data) {
    var template = this.options.get('templateSelection');
    var escapeMarkup = this.options.get('escapeMarkup');

    return escapeMarkup(template(data));
  };

  SingleSelection.prototype.selectionContainer = function () {
    return $('<span></span>');
  };

  SingleSelection.prototype.update = function (data) {
    if (data.length === 0) {
      this.clear();
      return;
    }

    var selection = data[0];

    var formatted = this.display(selection);

    var $rendered = this.$selection.find('.select2-selection__rendered');
    $rendered.empty().append(formatted);
    $rendered.prop('title', selection.title || selection.text);
  };

  return SingleSelection;
});

S2.define('select2/selection/multiple',[
  'jquery',
  './base',
  '../utils'
], function ($, BaseSelection, Utils) {
  function MultipleSelection ($element, options) {
    MultipleSelection.__super__.constructor.apply(this, arguments);
  }

  Utils.Extend(MultipleSelection, BaseSelection);

  MultipleSelection.prototype.render = function () {
    var $selection = MultipleSelection.__super__.render.call(this);

    $selection.addClass('select2-selection--multiple');

    $selection.html(
      '<ul class="select2-selection__rendered"></ul>'
    );

    return $selection;
  };

  MultipleSelection.prototype.bind = function (container, $container) {
    var self = this;

    MultipleSelection.__super__.bind.apply(this, arguments);

    this.$selection.on('click', function (evt) {
      self.trigger('toggle', {
        originalEvent: evt
      });
    });

    this.$selection.on('click', '.select2-selection__choice__remove',
      function (evt) {
      var $remove = $(this);
      var $selection = $remove.parent();

      var data = $selection.data('data');

      self.trigger('unselect', {
        originalEvent: evt,
        data: data
      });
    });
  };

  MultipleSelection.prototype.clear = function () {
    this.$selection.find('.select2-selection__rendered').empty();
  };

  MultipleSelection.prototype.display = function (data) {
    var template = this.options.get('templateSelection');
    var escapeMarkup = this.options.get('escapeMarkup');

    return escapeMarkup(template(data));
  };

  MultipleSelection.prototype.selectionContainer = function () {
    var $container = $(
      '<li class="select2-selection__choice">' +
        '<span class="select2-selection__choice__remove" role="presentation">' +
          '&times;' +
        '</span>' +
      '</li>'
    );

    return $container;
  };

  MultipleSelection.prototype.update = function (data) {
    this.clear();

    if (data.length === 0) {
      return;
    }

    var $selections = [];

    for (var d = 0; d < data.length; d++) {
      var selection = data[d];

      var formatted = this.display(selection);
      var $selection = this.selectionContainer();

      $selection.append(formatted);
      $selection.prop('title', selection.title || selection.text);

      $selection.data('data', selection);

      $selections.push($selection);
    }

    var $rendered = this.$selection.find('.select2-selection__rendered');

    Utils.appendMany($rendered, $selections);
  };

  return MultipleSelection;
});

S2.define('select2/selection/placeholder',[
  '../utils'
], function (Utils) {
  function Placeholder (decorated, $element, options) {
    this.placeholder = this.normalizePlaceholder(options.get('placeholder'));

    decorated.call(this, $element, options);
  }

  Placeholder.prototype.normalizePlaceholder = function (_, placeholder) {
    if (typeof placeholder === 'string') {
      placeholder = {
        id: '',
        text: placeholder
      };
    }

    return placeholder;
  };

  Placeholder.prototype.createPlaceholder = function (decorated, placeholder) {
    var $placeholder = this.selectionContainer();

    $placeholder.html(this.display(placeholder));
    $placeholder.addClass('select2-selection__placeholder')
                .removeClass('select2-selection__choice');

    return $placeholder;
  };

  Placeholder.prototype.update = function (decorated, data) {
    var singlePlaceholder = (
      data.length == 1 && data[0].id != this.placeholder.id
    );
    var multipleSelections = data.length > 1;

    if (multipleSelections || singlePlaceholder) {
      return decorated.call(this, data);
    }

    this.clear();

    var $placeholder = this.createPlaceholder(this.placeholder);

    this.$selection.find('.select2-selection__rendered').append($placeholder);
  };

  return Placeholder;
});

S2.define('select2/selection/allowClear',[
  'jquery',
  '../keys'
], function ($, KEYS) {
  function AllowClear () { }

  AllowClear.prototype.bind = function (decorated, container, $container) {
    var self = this;

    decorated.call(this, container, $container);

    if (this.placeholder == null) {
      if (this.options.get('debug') && window.console && console.error) {
        console.error(
          'Select2: The `allowClear` option should be used in combination ' +
          'with the `placeholder` option.'
        );
      }
    }

    this.$selection.on('mousedown', '.select2-selection__clear',
      function (evt) {
        self._handleClear(evt);
    });

    container.on('keypress', function (evt) {
      self._handleKeyboardClear(evt, container);
    });
  };

  AllowClear.prototype._handleClear = function (_, evt) {
    // Ignore the event if it is disabled
    if (this.options.get('disabled')) {
      return;
    }

    var $clear = this.$selection.find('.select2-selection__clear');

    // Ignore the event if nothing has been selected
    if ($clear.length === 0) {
      return;
    }

    evt.stopPropagation();

    var data = $clear.data('data');

    for (var d = 0; d < data.length; d++) {
      var unselectData = {
        data: data[d]
      };

      // Trigger the `unselect` event, so people can prevent it from being
      // cleared.
      this.trigger('unselect', unselectData);

      // If the event was prevented, don't clear it out.
      if (unselectData.prevented) {
        return;
      }
    }

    this.$element.val(this.placeholder.id).trigger('change');

    this.trigger('toggle');
  };

  AllowClear.prototype._handleKeyboardClear = function (_, evt, container) {
    if (container.isOpen()) {
      return;
    }

    if (evt.which == KEYS.DELETE || evt.which == KEYS.BACKSPACE) {
      this._handleClear(evt);
    }
  };

  AllowClear.prototype.update = function (decorated, data) {
    decorated.call(this, data);

    if (this.$selection.find('.select2-selection__placeholder').length > 0 ||
        data.length === 0) {
      return;
    }

    var $remove = $(
      '<span class="select2-selection__clear">' +
        '&times;' +
      '</span>'
    );
    $remove.data('data', data);

    this.$selection.find('.select2-selection__rendered').prepend($remove);
  };

  return AllowClear;
});

S2.define('select2/selection/search',[
  'jquery',
  '../utils',
  '../keys'
], function ($, Utils, KEYS) {
  function Search (decorated, $element, options) {
    decorated.call(this, $element, options);
  }

  Search.prototype.render = function (decorated) {
    var $search = $(
      '<li class="select2-search select2-search--inline">' +
        '<input class="select2-search__field" type="search" tabindex="-1"' +
        ' autocomplete="off" autocorrect="off" autocapitalize="off"' +
        ' spellcheck="false" role="textbox" />' +
      '</li>'
    );

    this.$searchContainer = $search;
    this.$search = $search.find('input');

    var $rendered = decorated.call(this);

    return $rendered;
  };

  Search.prototype.bind = function (decorated, container, $container) {
    var self = this;

    decorated.call(this, container, $container);

    container.on('open', function () {
      self.$search.attr('tabindex', 0);

      self.$search.focus();
    });

    container.on('close', function () {
      self.$search.attr('tabindex', -1);

      self.$search.val('');
      self.$search.focus();
    });

    container.on('enable', function () {
      self.$search.prop('disabled', false);
    });

    container.on('disable', function () {
      self.$search.prop('disabled', true);
    });

    this.$selection.on('focusin', '.select2-search--inline', function (evt) {
      self.trigger('focus', evt);
    });

    this.$selection.on('focusout', '.select2-search--inline', function (evt) {
      self.trigger('blur', evt);
    });

    this.$selection.on('keydown', '.select2-search--inline', function (evt) {
      evt.stopPropagation();

      self.trigger('keypress', evt);

      self._keyUpPrevented = evt.isDefaultPrevented();

      var key = evt.which;

      if (key === KEYS.BACKSPACE && self.$search.val() === '') {
        var $previousChoice = self.$searchContainer
          .prev('.select2-selection__choice');

        if ($previousChoice.length > 0) {
          var item = $previousChoice.data('data');

          self.searchRemoveChoice(item);

          evt.preventDefault();
        }
      }
    });

    // Workaround for browsers which do not support the `input` event
    // This will prevent double-triggering of events for browsers which support
    // both the `keyup` and `input` events.
    this.$selection.on('input', '.select2-search--inline', function (evt) {
      // Unbind the duplicated `keyup` event
      self.$selection.off('keyup.search');
    });

    this.$selection.on('keyup.search input', '.select2-search--inline',
        function (evt) {
      self.handleSearch(evt);
    });
  };

  Search.prototype.createPlaceholder = function (decorated, placeholder) {
    this.$search.attr('placeholder', placeholder.text);
  };

  Search.prototype.update = function (decorated, data) {
    this.$search.attr('placeholder', '');

    decorated.call(this, data);

    this.$selection.find('.select2-selection__rendered')
                   .append(this.$searchContainer);

    this.resizeSearch();
  };

  Search.prototype.handleSearch = function () {
    this.resizeSearch();

    if (!this._keyUpPrevented) {
      var input = this.$search.val();

      this.trigger('query', {
        term: input
      });
    }

    this._keyUpPrevented = false;
  };

  Search.prototype.searchRemoveChoice = function (decorated, item) {
    this.trigger('unselect', {
      data: item
    });

    this.trigger('open');

    this.$search.val(item.text + ' ');
  };

  Search.prototype.resizeSearch = function () {
    this.$search.css('width', '25px');

    var width = '';

    if (this.$search.attr('placeholder') !== '') {
      width = this.$selection.find('.select2-selection__rendered').innerWidth();
    } else {
      var minimumWidth = this.$search.val().length + 1;

      width = (minimumWidth * 0.75) + 'em';
    }

    this.$search.css('width', width);
  };

  return Search;
});

S2.define('select2/selection/eventRelay',[
  'jquery'
], function ($) {
  function EventRelay () { }

  EventRelay.prototype.bind = function (decorated, container, $container) {
    var self = this;
    var relayEvents = [
      'open', 'opening',
      'close', 'closing',
      'select', 'selecting',
      'unselect', 'unselecting'
    ];

    var preventableEvents = ['opening', 'closing', 'selecting', 'unselecting'];

    decorated.call(this, container, $container);

    container.on('*', function (name, params) {
      // Ignore events that should not be relayed
      if ($.inArray(name, relayEvents) === -1) {
        return;
      }

      // The parameters should always be an object
      params = params || {};

      // Generate the jQuery event for the Select2 event
      var evt = $.Event('select2:' + name, {
        params: params
      });

      self.$element.trigger(evt);

      // Only handle preventable events if it was one
      if ($.inArray(name, preventableEvents) === -1) {
        return;
      }

      params.prevented = evt.isDefaultPrevented();
    });
  };

  return EventRelay;
});

S2.define('select2/translation',[
  'jquery',
  'require'
], function ($, require) {
  function Translation (dict) {
    this.dict = dict || {};
  }

  Translation.prototype.all = function () {
    return this.dict;
  };

  Translation.prototype.get = function (key) {
    return this.dict[key];
  };

  Translation.prototype.extend = function (translation) {
    this.dict = $.extend({}, translation.all(), this.dict);
  };

  // Static functions

  Translation._cache = {};

  Translation.loadPath = function (path) {
    if (!(path in Translation._cache)) {
      var translations = require(path);

      Translation._cache[path] = translations;
    }

    return new Translation(Translation._cache[path]);
  };

  return Translation;
});

S2.define('select2/diacritics',[

], function () {
  var diacritics = {
    '\u24B6': 'A',
    '\uFF21': 'A',
    '\u00C0': 'A',
    '\u00C1': 'A',
    '\u00C2': 'A',
    '\u1EA6': 'A',
    '\u1EA4': 'A',
    '\u1EAA': 'A',
    '\u1EA8': 'A',
    '\u00C3': 'A',
    '\u0100': 'A',
    '\u0102': 'A',
    '\u1EB0': 'A',
    '\u1EAE': 'A',
    '\u1EB4': 'A',
    '\u1EB2': 'A',
    '\u0226': 'A',
    '\u01E0': 'A',
    '\u00C4': 'A',
    '\u01DE': 'A',
    '\u1EA2': 'A',
    '\u00C5': 'A',
    '\u01FA': 'A',
    '\u01CD': 'A',
    '\u0200': 'A',
    '\u0202': 'A',
    '\u1EA0': 'A',
    '\u1EAC': 'A',
    '\u1EB6': 'A',
    '\u1E00': 'A',
    '\u0104': 'A',
    '\u023A': 'A',
    '\u2C6F': 'A',
    '\uA732': 'AA',
    '\u00C6': 'AE',
    '\u01FC': 'AE',
    '\u01E2': 'AE',
    '\uA734': 'AO',
    '\uA736': 'AU',
    '\uA738': 'AV',
    '\uA73A': 'AV',
    '\uA73C': 'AY',
    '\u24B7': 'B',
    '\uFF22': 'B',
    '\u1E02': 'B',
    '\u1E04': 'B',
    '\u1E06': 'B',
    '\u0243': 'B',
    '\u0182': 'B',
    '\u0181': 'B',
    '\u24B8': 'C',
    '\uFF23': 'C',
    '\u0106': 'C',
    '\u0108': 'C',
    '\u010A': 'C',
    '\u010C': 'C',
    '\u00C7': 'C',
    '\u1E08': 'C',
    '\u0187': 'C',
    '\u023B': 'C',
    '\uA73E': 'C',
    '\u24B9': 'D',
    '\uFF24': 'D',
    '\u1E0A': 'D',
    '\u010E': 'D',
    '\u1E0C': 'D',
    '\u1E10': 'D',
    '\u1E12': 'D',
    '\u1E0E': 'D',
    '\u0110': 'D',
    '\u018B': 'D',
    '\u018A': 'D',
    '\u0189': 'D',
    '\uA779': 'D',
    '\u01F1': 'DZ',
    '\u01C4': 'DZ',
    '\u01F2': 'Dz',
    '\u01C5': 'Dz',
    '\u24BA': 'E',
    '\uFF25': 'E',
    '\u00C8': 'E',
    '\u00C9': 'E',
    '\u00CA': 'E',
    '\u1EC0': 'E',
    '\u1EBE': 'E',
    '\u1EC4': 'E',
    '\u1EC2': 'E',
    '\u1EBC': 'E',
    '\u0112': 'E',
    '\u1E14': 'E',
    '\u1E16': 'E',
    '\u0114': 'E',
    '\u0116': 'E',
    '\u00CB': 'E',
    '\u1EBA': 'E',
    '\u011A': 'E',
    '\u0204': 'E',
    '\u0206': 'E',
    '\u1EB8': 'E',
    '\u1EC6': 'E',
    '\u0228': 'E',
    '\u1E1C': 'E',
    '\u0118': 'E',
    '\u1E18': 'E',
    '\u1E1A': 'E',
    '\u0190': 'E',
    '\u018E': 'E',
    '\u24BB': 'F',
    '\uFF26': 'F',
    '\u1E1E': 'F',
    '\u0191': 'F',
    '\uA77B': 'F',
    '\u24BC': 'G',
    '\uFF27': 'G',
    '\u01F4': 'G',
    '\u011C': 'G',
    '\u1E20': 'G',
    '\u011E': 'G',
    '\u0120': 'G',
    '\u01E6': 'G',
    '\u0122': 'G',
    '\u01E4': 'G',
    '\u0193': 'G',
    '\uA7A0': 'G',
    '\uA77D': 'G',
    '\uA77E': 'G',
    '\u24BD': 'H',
    '\uFF28': 'H',
    '\u0124': 'H',
    '\u1E22': 'H',
    '\u1E26': 'H',
    '\u021E': 'H',
    '\u1E24': 'H',
    '\u1E28': 'H',
    '\u1E2A': 'H',
    '\u0126': 'H',
    '\u2C67': 'H',
    '\u2C75': 'H',
    '\uA78D': 'H',
    '\u24BE': 'I',
    '\uFF29': 'I',
    '\u00CC': 'I',
    '\u00CD': 'I',
    '\u00CE': 'I',
    '\u0128': 'I',
    '\u012A': 'I',
    '\u012C': 'I',
    '\u0130': 'I',
    '\u00CF': 'I',
    '\u1E2E': 'I',
    '\u1EC8': 'I',
    '\u01CF': 'I',
    '\u0208': 'I',
    '\u020A': 'I',
    '\u1ECA': 'I',
    '\u012E': 'I',
    '\u1E2C': 'I',
    '\u0197': 'I',
    '\u24BF': 'J',
    '\uFF2A': 'J',
    '\u0134': 'J',
    '\u0248': 'J',
    '\u24C0': 'K',
    '\uFF2B': 'K',
    '\u1E30': 'K',
    '\u01E8': 'K',
    '\u1E32': 'K',
    '\u0136': 'K',
    '\u1E34': 'K',
    '\u0198': 'K',
    '\u2C69': 'K',
    '\uA740': 'K',
    '\uA742': 'K',
    '\uA744': 'K',
    '\uA7A2': 'K',
    '\u24C1': 'L',
    '\uFF2C': 'L',
    '\u013F': 'L',
    '\u0139': 'L',
    '\u013D': 'L',
    '\u1E36': 'L',
    '\u1E38': 'L',
    '\u013B': 'L',
    '\u1E3C': 'L',
    '\u1E3A': 'L',
    '\u0141': 'L',
    '\u023D': 'L',
    '\u2C62': 'L',
    '\u2C60': 'L',
    '\uA748': 'L',
    '\uA746': 'L',
    '\uA780': 'L',
    '\u01C7': 'LJ',
    '\u01C8': 'Lj',
    '\u24C2': 'M',
    '\uFF2D': 'M',
    '\u1E3E': 'M',
    '\u1E40': 'M',
    '\u1E42': 'M',
    '\u2C6E': 'M',
    '\u019C': 'M',
    '\u24C3': 'N',
    '\uFF2E': 'N',
    '\u01F8': 'N',
    '\u0143': 'N',
    '\u00D1': 'N',
    '\u1E44': 'N',
    '\u0147': 'N',
    '\u1E46': 'N',
    '\u0145': 'N',
    '\u1E4A': 'N',
    '\u1E48': 'N',
    '\u0220': 'N',
    '\u019D': 'N',
    '\uA790': 'N',
    '\uA7A4': 'N',
    '\u01CA': 'NJ',
    '\u01CB': 'Nj',
    '\u24C4': 'O',
    '\uFF2F': 'O',
    '\u00D2': 'O',
    '\u00D3': 'O',
    '\u00D4': 'O',
    '\u1ED2': 'O',
    '\u1ED0': 'O',
    '\u1ED6': 'O',
    '\u1ED4': 'O',
    '\u00D5': 'O',
    '\u1E4C': 'O',
    '\u022C': 'O',
    '\u1E4E': 'O',
    '\u014C': 'O',
    '\u1E50': 'O',
    '\u1E52': 'O',
    '\u014E': 'O',
    '\u022E': 'O',
    '\u0230': 'O',
    '\u00D6': 'O',
    '\u022A': 'O',
    '\u1ECE': 'O',
    '\u0150': 'O',
    '\u01D1': 'O',
    '\u020C': 'O',
    '\u020E': 'O',
    '\u01A0': 'O',
    '\u1EDC': 'O',
    '\u1EDA': 'O',
    '\u1EE0': 'O',
    '\u1EDE': 'O',
    '\u1EE2': 'O',
    '\u1ECC': 'O',
    '\u1ED8': 'O',
    '\u01EA': 'O',
    '\u01EC': 'O',
    '\u00D8': 'O',
    '\u01FE': 'O',
    '\u0186': 'O',
    '\u019F': 'O',
    '\uA74A': 'O',
    '\uA74C': 'O',
    '\u01A2': 'OI',
    '\uA74E': 'OO',
    '\u0222': 'OU',
    '\u24C5': 'P',
    '\uFF30': 'P',
    '\u1E54': 'P',
    '\u1E56': 'P',
    '\u01A4': 'P',
    '\u2C63': 'P',
    '\uA750': 'P',
    '\uA752': 'P',
    '\uA754': 'P',
    '\u24C6': 'Q',
    '\uFF31': 'Q',
    '\uA756': 'Q',
    '\uA758': 'Q',
    '\u024A': 'Q',
    '\u24C7': 'R',
    '\uFF32': 'R',
    '\u0154': 'R',
    '\u1E58': 'R',
    '\u0158': 'R',
    '\u0210': 'R',
    '\u0212': 'R',
    '\u1E5A': 'R',
    '\u1E5C': 'R',
    '\u0156': 'R',
    '\u1E5E': 'R',
    '\u024C': 'R',
    '\u2C64': 'R',
    '\uA75A': 'R',
    '\uA7A6': 'R',
    '\uA782': 'R',
    '\u24C8': 'S',
    '\uFF33': 'S',
    '\u1E9E': 'S',
    '\u015A': 'S',
    '\u1E64': 'S',
    '\u015C': 'S',
    '\u1E60': 'S',
    '\u0160': 'S',
    '\u1E66': 'S',
    '\u1E62': 'S',
    '\u1E68': 'S',
    '\u0218': 'S',
    '\u015E': 'S',
    '\u2C7E': 'S',
    '\uA7A8': 'S',
    '\uA784': 'S',
    '\u24C9': 'T',
    '\uFF34': 'T',
    '\u1E6A': 'T',
    '\u0164': 'T',
    '\u1E6C': 'T',
    '\u021A': 'T',
    '\u0162': 'T',
    '\u1E70': 'T',
    '\u1E6E': 'T',
    '\u0166': 'T',
    '\u01AC': 'T',
    '\u01AE': 'T',
    '\u023E': 'T',
    '\uA786': 'T',
    '\uA728': 'TZ',
    '\u24CA': 'U',
    '\uFF35': 'U',
    '\u00D9': 'U',
    '\u00DA': 'U',
    '\u00DB': 'U',
    '\u0168': 'U',
    '\u1E78': 'U',
    '\u016A': 'U',
    '\u1E7A': 'U',
    '\u016C': 'U',
    '\u00DC': 'U',
    '\u01DB': 'U',
    '\u01D7': 'U',
    '\u01D5': 'U',
    '\u01D9': 'U',
    '\u1EE6': 'U',
    '\u016E': 'U',
    '\u0170': 'U',
    '\u01D3': 'U',
    '\u0214': 'U',
    '\u0216': 'U',
    '\u01AF': 'U',
    '\u1EEA': 'U',
    '\u1EE8': 'U',
    '\u1EEE': 'U',
    '\u1EEC': 'U',
    '\u1EF0': 'U',
    '\u1EE4': 'U',
    '\u1E72': 'U',
    '\u0172': 'U',
    '\u1E76': 'U',
    '\u1E74': 'U',
    '\u0244': 'U',
    '\u24CB': 'V',
    '\uFF36': 'V',
    '\u1E7C': 'V',
    '\u1E7E': 'V',
    '\u01B2': 'V',
    '\uA75E': 'V',
    '\u0245': 'V',
    '\uA760': 'VY',
    '\u24CC': 'W',
    '\uFF37': 'W',
    '\u1E80': 'W',
    '\u1E82': 'W',
    '\u0174': 'W',
    '\u1E86': 'W',
    '\u1E84': 'W',
    '\u1E88': 'W',
    '\u2C72': 'W',
    '\u24CD': 'X',
    '\uFF38': 'X',
    '\u1E8A': 'X',
    '\u1E8C': 'X',
    '\u24CE': 'Y',
    '\uFF39': 'Y',
    '\u1EF2': 'Y',
    '\u00DD': 'Y',
    '\u0176': 'Y',
    '\u1EF8': 'Y',
    '\u0232': 'Y',
    '\u1E8E': 'Y',
    '\u0178': 'Y',
    '\u1EF6': 'Y',
    '\u1EF4': 'Y',
    '\u01B3': 'Y',
    '\u024E': 'Y',
    '\u1EFE': 'Y',
    '\u24CF': 'Z',
    '\uFF3A': 'Z',
    '\u0179': 'Z',
    '\u1E90': 'Z',
    '\u017B': 'Z',
    '\u017D': 'Z',
    '\u1E92': 'Z',
    '\u1E94': 'Z',
    '\u01B5': 'Z',
    '\u0224': 'Z',
    '\u2C7F': 'Z',
    '\u2C6B': 'Z',
    '\uA762': 'Z',
    '\u24D0': 'a',
    '\uFF41': 'a',
    '\u1E9A': 'a',
    '\u00E0': 'a',
    '\u00E1': 'a',
    '\u00E2': 'a',
    '\u1EA7': 'a',
    '\u1EA5': 'a',
    '\u1EAB': 'a',
    '\u1EA9': 'a',
    '\u00E3': 'a',
    '\u0101': 'a',
    '\u0103': 'a',
    '\u1EB1': 'a',
    '\u1EAF': 'a',
    '\u1EB5': 'a',
    '\u1EB3': 'a',
    '\u0227': 'a',
    '\u01E1': 'a',
    '\u00E4': 'a',
    '\u01DF': 'a',
    '\u1EA3': 'a',
    '\u00E5': 'a',
    '\u01FB': 'a',
    '\u01CE': 'a',
    '\u0201': 'a',
    '\u0203': 'a',
    '\u1EA1': 'a',
    '\u1EAD': 'a',
    '\u1EB7': 'a',
    '\u1E01': 'a',
    '\u0105': 'a',
    '\u2C65': 'a',
    '\u0250': 'a',
    '\uA733': 'aa',
    '\u00E6': 'ae',
    '\u01FD': 'ae',
    '\u01E3': 'ae',
    '\uA735': 'ao',
    '\uA737': 'au',
    '\uA739': 'av',
    '\uA73B': 'av',
    '\uA73D': 'ay',
    '\u24D1': 'b',
    '\uFF42': 'b',
    '\u1E03': 'b',
    '\u1E05': 'b',
    '\u1E07': 'b',
    '\u0180': 'b',
    '\u0183': 'b',
    '\u0253': 'b',
    '\u24D2': 'c',
    '\uFF43': 'c',
    '\u0107': 'c',
    '\u0109': 'c',
    '\u010B': 'c',
    '\u010D': 'c',
    '\u00E7': 'c',
    '\u1E09': 'c',
    '\u0188': 'c',
    '\u023C': 'c',
    '\uA73F': 'c',
    '\u2184': 'c',
    '\u24D3': 'd',
    '\uFF44': 'd',
    '\u1E0B': 'd',
    '\u010F': 'd',
    '\u1E0D': 'd',
    '\u1E11': 'd',
    '\u1E13': 'd',
    '\u1E0F': 'd',
    '\u0111': 'd',
    '\u018C': 'd',
    '\u0256': 'd',
    '\u0257': 'd',
    '\uA77A': 'd',
    '\u01F3': 'dz',
    '\u01C6': 'dz',
    '\u24D4': 'e',
    '\uFF45': 'e',
    '\u00E8': 'e',
    '\u00E9': 'e',
    '\u00EA': 'e',
    '\u1EC1': 'e',
    '\u1EBF': 'e',
    '\u1EC5': 'e',
    '\u1EC3': 'e',
    '\u1EBD': 'e',
    '\u0113': 'e',
    '\u1E15': 'e',
    '\u1E17': 'e',
    '\u0115': 'e',
    '\u0117': 'e',
    '\u00EB': 'e',
    '\u1EBB': 'e',
    '\u011B': 'e',
    '\u0205': 'e',
    '\u0207': 'e',
    '\u1EB9': 'e',
    '\u1EC7': 'e',
    '\u0229': 'e',
    '\u1E1D': 'e',
    '\u0119': 'e',
    '\u1E19': 'e',
    '\u1E1B': 'e',
    '\u0247': 'e',
    '\u025B': 'e',
    '\u01DD': 'e',
    '\u24D5': 'f',
    '\uFF46': 'f',
    '\u1E1F': 'f',
    '\u0192': 'f',
    '\uA77C': 'f',
    '\u24D6': 'g',
    '\uFF47': 'g',
    '\u01F5': 'g',
    '\u011D': 'g',
    '\u1E21': 'g',
    '\u011F': 'g',
    '\u0121': 'g',
    '\u01E7': 'g',
    '\u0123': 'g',
    '\u01E5': 'g',
    '\u0260': 'g',
    '\uA7A1': 'g',
    '\u1D79': 'g',
    '\uA77F': 'g',
    '\u24D7': 'h',
    '\uFF48': 'h',
    '\u0125': 'h',
    '\u1E23': 'h',
    '\u1E27': 'h',
    '\u021F': 'h',
    '\u1E25': 'h',
    '\u1E29': 'h',
    '\u1E2B': 'h',
    '\u1E96': 'h',
    '\u0127': 'h',
    '\u2C68': 'h',
    '\u2C76': 'h',
    '\u0265': 'h',
    '\u0195': 'hv',
    '\u24D8': 'i',
    '\uFF49': 'i',
    '\u00EC': 'i',
    '\u00ED': 'i',
    '\u00EE': 'i',
    '\u0129': 'i',
    '\u012B': 'i',
    '\u012D': 'i',
    '\u00EF': 'i',
    '\u1E2F': 'i',
    '\u1EC9': 'i',
    '\u01D0': 'i',
    '\u0209': 'i',
    '\u020B': 'i',
    '\u1ECB': 'i',
    '\u012F': 'i',
    '\u1E2D': 'i',
    '\u0268': 'i',
    '\u0131': 'i',
    '\u24D9': 'j',
    '\uFF4A': 'j',
    '\u0135': 'j',
    '\u01F0': 'j',
    '\u0249': 'j',
    '\u24DA': 'k',
    '\uFF4B': 'k',
    '\u1E31': 'k',
    '\u01E9': 'k',
    '\u1E33': 'k',
    '\u0137': 'k',
    '\u1E35': 'k',
    '\u0199': 'k',
    '\u2C6A': 'k',
    '\uA741': 'k',
    '\uA743': 'k',
    '\uA745': 'k',
    '\uA7A3': 'k',
    '\u24DB': 'l',
    '\uFF4C': 'l',
    '\u0140': 'l',
    '\u013A': 'l',
    '\u013E': 'l',
    '\u1E37': 'l',
    '\u1E39': 'l',
    '\u013C': 'l',
    '\u1E3D': 'l',
    '\u1E3B': 'l',
    '\u017F': 'l',
    '\u0142': 'l',
    '\u019A': 'l',
    '\u026B': 'l',
    '\u2C61': 'l',
    '\uA749': 'l',
    '\uA781': 'l',
    '\uA747': 'l',
    '\u01C9': 'lj',
    '\u24DC': 'm',
    '\uFF4D': 'm',
    '\u1E3F': 'm',
    '\u1E41': 'm',
    '\u1E43': 'm',
    '\u0271': 'm',
    '\u026F': 'm',
    '\u24DD': 'n',
    '\uFF4E': 'n',
    '\u01F9': 'n',
    '\u0144': 'n',
    '\u00F1': 'n',
    '\u1E45': 'n',
    '\u0148': 'n',
    '\u1E47': 'n',
    '\u0146': 'n',
    '\u1E4B': 'n',
    '\u1E49': 'n',
    '\u019E': 'n',
    '\u0272': 'n',
    '\u0149': 'n',
    '\uA791': 'n',
    '\uA7A5': 'n',
    '\u01CC': 'nj',
    '\u24DE': 'o',
    '\uFF4F': 'o',
    '\u00F2': 'o',
    '\u00F3': 'o',
    '\u00F4': 'o',
    '\u1ED3': 'o',
    '\u1ED1': 'o',
    '\u1ED7': 'o',
    '\u1ED5': 'o',
    '\u00F5': 'o',
    '\u1E4D': 'o',
    '\u022D': 'o',
    '\u1E4F': 'o',
    '\u014D': 'o',
    '\u1E51': 'o',
    '\u1E53': 'o',
    '\u014F': 'o',
    '\u022F': 'o',
    '\u0231': 'o',
    '\u00F6': 'o',
    '\u022B': 'o',
    '\u1ECF': 'o',
    '\u0151': 'o',
    '\u01D2': 'o',
    '\u020D': 'o',
    '\u020F': 'o',
    '\u01A1': 'o',
    '\u1EDD': 'o',
    '\u1EDB': 'o',
    '\u1EE1': 'o',
    '\u1EDF': 'o',
    '\u1EE3': 'o',
    '\u1ECD': 'o',
    '\u1ED9': 'o',
    '\u01EB': 'o',
    '\u01ED': 'o',
    '\u00F8': 'o',
    '\u01FF': 'o',
    '\u0254': 'o',
    '\uA74B': 'o',
    '\uA74D': 'o',
    '\u0275': 'o',
    '\u01A3': 'oi',
    '\u0223': 'ou',
    '\uA74F': 'oo',
    '\u24DF': 'p',
    '\uFF50': 'p',
    '\u1E55': 'p',
    '\u1E57': 'p',
    '\u01A5': 'p',
    '\u1D7D': 'p',
    '\uA751': 'p',
    '\uA753': 'p',
    '\uA755': 'p',
    '\u24E0': 'q',
    '\uFF51': 'q',
    '\u024B': 'q',
    '\uA757': 'q',
    '\uA759': 'q',
    '\u24E1': 'r',
    '\uFF52': 'r',
    '\u0155': 'r',
    '\u1E59': 'r',
    '\u0159': 'r',
    '\u0211': 'r',
    '\u0213': 'r',
    '\u1E5B': 'r',
    '\u1E5D': 'r',
    '\u0157': 'r',
    '\u1E5F': 'r',
    '\u024D': 'r',
    '\u027D': 'r',
    '\uA75B': 'r',
    '\uA7A7': 'r',
    '\uA783': 'r',
    '\u24E2': 's',
    '\uFF53': 's',
    '\u00DF': 's',
    '\u015B': 's',
    '\u1E65': 's',
    '\u015D': 's',
    '\u1E61': 's',
    '\u0161': 's',
    '\u1E67': 's',
    '\u1E63': 's',
    '\u1E69': 's',
    '\u0219': 's',
    '\u015F': 's',
    '\u023F': 's',
    '\uA7A9': 's',
    '\uA785': 's',
    '\u1E9B': 's',
    '\u24E3': 't',
    '\uFF54': 't',
    '\u1E6B': 't',
    '\u1E97': 't',
    '\u0165': 't',
    '\u1E6D': 't',
    '\u021B': 't',
    '\u0163': 't',
    '\u1E71': 't',
    '\u1E6F': 't',
    '\u0167': 't',
    '\u01AD': 't',
    '\u0288': 't',
    '\u2C66': 't',
    '\uA787': 't',
    '\uA729': 'tz',
    '\u24E4': 'u',
    '\uFF55': 'u',
    '\u00F9': 'u',
    '\u00FA': 'u',
    '\u00FB': 'u',
    '\u0169': 'u',
    '\u1E79': 'u',
    '\u016B': 'u',
    '\u1E7B': 'u',
    '\u016D': 'u',
    '\u00FC': 'u',
    '\u01DC': 'u',
    '\u01D8': 'u',
    '\u01D6': 'u',
    '\u01DA': 'u',
    '\u1EE7': 'u',
    '\u016F': 'u',
    '\u0171': 'u',
    '\u01D4': 'u',
    '\u0215': 'u',
    '\u0217': 'u',
    '\u01B0': 'u',
    '\u1EEB': 'u',
    '\u1EE9': 'u',
    '\u1EEF': 'u',
    '\u1EED': 'u',
    '\u1EF1': 'u',
    '\u1EE5': 'u',
    '\u1E73': 'u',
    '\u0173': 'u',
    '\u1E77': 'u',
    '\u1E75': 'u',
    '\u0289': 'u',
    '\u24E5': 'v',
    '\uFF56': 'v',
    '\u1E7D': 'v',
    '\u1E7F': 'v',
    '\u028B': 'v',
    '\uA75F': 'v',
    '\u028C': 'v',
    '\uA761': 'vy',
    '\u24E6': 'w',
    '\uFF57': 'w',
    '\u1E81': 'w',
    '\u1E83': 'w',
    '\u0175': 'w',
    '\u1E87': 'w',
    '\u1E85': 'w',
    '\u1E98': 'w',
    '\u1E89': 'w',
    '\u2C73': 'w',
    '\u24E7': 'x',
    '\uFF58': 'x',
    '\u1E8B': 'x',
    '\u1E8D': 'x',
    '\u24E8': 'y',
    '\uFF59': 'y',
    '\u1EF3': 'y',
    '\u00FD': 'y',
    '\u0177': 'y',
    '\u1EF9': 'y',
    '\u0233': 'y',
    '\u1E8F': 'y',
    '\u00FF': 'y',
    '\u1EF7': 'y',
    '\u1E99': 'y',
    '\u1EF5': 'y',
    '\u01B4': 'y',
    '\u024F': 'y',
    '\u1EFF': 'y',
    '\u24E9': 'z',
    '\uFF5A': 'z',
    '\u017A': 'z',
    '\u1E91': 'z',
    '\u017C': 'z',
    '\u017E': 'z',
    '\u1E93': 'z',
    '\u1E95': 'z',
    '\u01B6': 'z',
    '\u0225': 'z',
    '\u0240': 'z',
    '\u2C6C': 'z',
    '\uA763': 'z',
    '\u0386': '\u0391',
    '\u0388': '\u0395',
    '\u0389': '\u0397',
    '\u038A': '\u0399',
    '\u03AA': '\u0399',
    '\u038C': '\u039F',
    '\u038E': '\u03A5',
    '\u03AB': '\u03A5',
    '\u038F': '\u03A9',
    '\u03AC': '\u03B1',
    '\u03AD': '\u03B5',
    '\u03AE': '\u03B7',
    '\u03AF': '\u03B9',
    '\u03CA': '\u03B9',
    '\u0390': '\u03B9',
    '\u03CC': '\u03BF',
    '\u03CD': '\u03C5',
    '\u03CB': '\u03C5',
    '\u03B0': '\u03C5',
    '\u03C9': '\u03C9',
    '\u03C2': '\u03C3'
  };

  return diacritics;
});

S2.define('select2/data/base',[
  '../utils'
], function (Utils) {
  function BaseAdapter ($element, options) {
    BaseAdapter.__super__.constructor.call(this);
  }

  Utils.Extend(BaseAdapter, Utils.Observable);

  BaseAdapter.prototype.current = function (callback) {
    throw new Error('The `current` method must be defined in child classes.');
  };

  BaseAdapter.prototype.query = function (params, callback) {
    throw new Error('The `query` method must be defined in child classes.');
  };

  BaseAdapter.prototype.bind = function (container, $container) {
    // Can be implemented in subclasses
  };

  BaseAdapter.prototype.destroy = function () {
    // Can be implemented in subclasses
  };

  BaseAdapter.prototype.generateResultId = function (container, data) {
    var id = container.id + '-result-';

    id += Utils.generateChars(4);

    if (data.id != null) {
      id += '-' + data.id.toString();
    } else {
      id += '-' + Utils.generateChars(4);
    }
    return id;
  };

  return BaseAdapter;
});

S2.define('select2/data/select',[
  './base',
  '../utils',
  'jquery'
], function (BaseAdapter, Utils, $) {
  function SelectAdapter ($element, options) {
    this.$element = $element;
    this.options = options;

    SelectAdapter.__super__.constructor.call(this);
  }

  Utils.Extend(SelectAdapter, BaseAdapter);

  SelectAdapter.prototype.current = function (callback) {
    var data = [];
    var self = this;

    this.$element.find(':selected').each(function () {
      var $option = $(this);

      var option = self.item($option);

      data.push(option);
    });

    callback(data);
  };

  SelectAdapter.prototype.select = function (data) {
    var self = this;

    data.selected = true;

    // If data.element is a DOM node, use it instead
    if ($(data.element).is('option')) {
      data.element.selected = true;

      this.$element.trigger('change');

      return;
    }

    if (this.$element.prop('multiple')) {
      this.current(function (currentData) {
        var val = [];

        data = [data];
        data.push.apply(data, currentData);

        for (var d = 0; d < data.length; d++) {
          var id = data[d].id;

          if ($.inArray(id, val) === -1) {
            val.push(id);
          }
        }

        self.$element.val(val);
        self.$element.trigger('change');
      });
    } else {
      var val = data.id;

      this.$element.val(val);
      this.$element.trigger('change');
    }
  };

  SelectAdapter.prototype.unselect = function (data) {
    var self = this;

    if (!this.$element.prop('multiple')) {
      return;
    }

    data.selected = false;

    if ($(data.element).is('option')) {
      data.element.selected = false;

      this.$element.trigger('change');

      return;
    }

    this.current(function (currentData) {
      var val = [];

      for (var d = 0; d < currentData.length; d++) {
        var id = currentData[d].id;

        if (id !== data.id && $.inArray(id, val) === -1) {
          val.push(id);
        }
      }

      self.$element.val(val);

      self.$element.trigger('change');
    });
  };

  SelectAdapter.prototype.bind = function (container, $container) {
    var self = this;

    this.container = container;

    container.on('select', function (params) {
      self.select(params.data);
    });

    container.on('unselect', function (params) {
      self.unselect(params.data);
    });
  };

  SelectAdapter.prototype.destroy = function () {
    // Remove anything added to child elements
    this.$element.find('*').each(function () {
      // Remove any custom data set by Select2
      $.removeData(this, 'data');
    });
  };

  SelectAdapter.prototype.query = function (params, callback) {
    var data = [];
    var self = this;

    var $options = this.$element.children();

    $options.each(function () {
      var $option = $(this);

      if (!$option.is('option') && !$option.is('optgroup')) {
        return;
      }

      var option = self.item($option);

      var matches = self.matches(params, option);

      if (matches !== null) {
        data.push(matches);
      }
    });

    callback({
      results: data
    });
  };

  SelectAdapter.prototype.addOptions = function ($options) {
    Utils.appendMany(this.$element, $options);
  };

  SelectAdapter.prototype.option = function (data) {
    var option;

    if (data.children) {
      option = document.createElement('optgroup');
      option.label = data.text;
    } else {
      option = document.createElement('option');

      if (option.textContent !== undefined) {
        option.textContent = data.text;
      } else {
        option.innerText = data.text;
      }
    }

    if (data.id) {
      option.value = data.id;
    }

    if (data.disabled) {
      option.disabled = true;
    }

    if (data.selected) {
      option.selected = true;
    }

    if (data.title) {
      option.title = data.title;
    }

    var $option = $(option);

    var normalizedData = this._normalizeItem(data);
    normalizedData.element = option;

    // Override the option's data with the combined data
    $.data(option, 'data', normalizedData);

    return $option;
  };

  SelectAdapter.prototype.item = function ($option) {
    var data = {};

    data = $.data($option[0], 'data');

    if (data != null) {
      return data;
    }

    if ($option.is('option')) {
      data = {
        id: $option.val(),
        text: $option.text(),
        disabled: $option.prop('disabled'),
        selected: $option.prop('selected'),
        title: $option.prop('title')
      };
    } else if ($option.is('optgroup')) {
      data = {
        text: $option.prop('label'),
        children: [],
        title: $option.prop('title')
      };

      var $children = $option.children('option');
      var children = [];

      for (var c = 0; c < $children.length; c++) {
        var $child = $($children[c]);

        var child = this.item($child);

        children.push(child);
      }

      data.children = children;
    }

    data = this._normalizeItem(data);
    data.element = $option[0];

    $.data($option[0], 'data', data);

    return data;
  };

  SelectAdapter.prototype._normalizeItem = function (item) {
    if (!$.isPlainObject(item)) {
      item = {
        id: item,
        text: item
      };
    }

    item = $.extend({}, {
      text: ''
    }, item);

    var defaults = {
      selected: false,
      disabled: false
    };

    if (item.id != null) {
      item.id = item.id.toString();
    }

    if (item.text != null) {
      item.text = item.text.toString();
    }

    if (item._resultId == null && item.id && this.container != null) {
      item._resultId = this.generateResultId(this.container, item);
    }

    return $.extend({}, defaults, item);
  };

  SelectAdapter.prototype.matches = function (params, data) {
    var matcher = this.options.get('matcher');

    return matcher(params, data);
  };

  return SelectAdapter;
});

S2.define('select2/data/array',[
  './select',
  '../utils',
  'jquery'
], function (SelectAdapter, Utils, $) {
  function ArrayAdapter ($element, options) {
    var data = options.get('data') || [];

    ArrayAdapter.__super__.constructor.call(this, $element, options);

    this.addOptions(this.convertToOptions(data));
  }

  Utils.Extend(ArrayAdapter, SelectAdapter);

  ArrayAdapter.prototype.select = function (data) {
    var $option = this.$element.find('option').filter(function (i, elm) {
      return elm.value == data.id.toString();
    });

    if ($option.length === 0) {
      $option = this.option(data);

      this.addOptions($option);
    }

    ArrayAdapter.__super__.select.call(this, data);
  };

  ArrayAdapter.prototype.convertToOptions = function (data) {
    var self = this;

    var $existing = this.$element.find('option');
    var existingIds = $existing.map(function () {
      return self.item($(this)).id;
    }).get();

    var $options = [];

    // Filter out all items except for the one passed in the argument
    function onlyItem (item) {
      return function () {
        return $(this).val() == item.id;
      };
    }

    for (var d = 0; d < data.length; d++) {
      var item = this._normalizeItem(data[d]);

      // Skip items which were pre-loaded, only merge the data
      if ($.inArray(item.id, existingIds) >= 0) {
        var $existingOption = $existing.filter(onlyItem(item));

        var existingData = this.item($existingOption);
        var newData = $.extend(true, {}, existingData, item);

        var $newOption = this.option(existingData);

        $existingOption.replaceWith($newOption);

        continue;
      }

      var $option = this.option(item);

      if (item.children) {
        var $children = this.convertToOptions(item.children);

        Utils.appendMany($option, $children);
      }

      $options.push($option);
    }

    return $options;
  };

  return ArrayAdapter;
});

S2.define('select2/data/ajax',[
  './array',
  '../utils',
  'jquery'
], function (ArrayAdapter, Utils, $) {
  function AjaxAdapter ($element, options) {
    this.ajaxOptions = this._applyDefaults(options.get('ajax'));

    if (this.ajaxOptions.processResults != null) {
      this.processResults = this.ajaxOptions.processResults;
    }

    ArrayAdapter.__super__.constructor.call(this, $element, options);
  }

  Utils.Extend(AjaxAdapter, ArrayAdapter);

  AjaxAdapter.prototype._applyDefaults = function (options) {
    var defaults = {
      data: function (params) {
        return {
          q: params.term
        };
      },
      transport: function (params, success, failure) {
        var $request = $.ajax(params);

        $request.then(success);
        $request.fail(failure);

        return $request;
      }
    };

    return $.extend({}, defaults, options, true);
  };

  AjaxAdapter.prototype.processResults = function (results) {
    return results;
  };

  AjaxAdapter.prototype.query = function (params, callback) {
    var matches = [];
    var self = this;

    if (this._request != null) {
      // JSONP requests cannot always be aborted
      if ($.isFunction(this._request.abort)) {
        this._request.abort();
      }

      this._request = null;
    }

    var options = $.extend({
      type: 'GET'
    }, this.ajaxOptions);

    if (typeof options.url === 'function') {
      options.url = options.url(params);
    }

    if (typeof options.data === 'function') {
      options.data = options.data(params);
    }

    function request () {
      var $request = options.transport(options, function (data) {
        var results = self.processResults(data, params);

        if (self.options.get('debug') && window.console && console.error) {
          // Check to make sure that the response included a `results` key.
          if (!results || !results.results || !$.isArray(results.results)) {
            console.error(
              'Select2: The AJAX results did not return an array in the ' +
              '`results` key of the response.'
            );
          }
        }

        callback(results);
      }, function () {
        // TODO: Handle AJAX errors
      });

      self._request = $request;
    }

    if (this.ajaxOptions.delay && params.term !== '') {
      if (this._queryTimeout) {
        window.clearTimeout(this._queryTimeout);
      }

      this._queryTimeout = window.setTimeout(request, this.ajaxOptions.delay);
    } else {
      request();
    }
  };

  return AjaxAdapter;
});

S2.define('select2/data/tags',[
  'jquery'
], function ($) {
  function Tags (decorated, $element, options) {
    var tags = options.get('tags');

    var createTag = options.get('createTag');

    if (createTag !== undefined) {
      this.createTag = createTag;
    }

    decorated.call(this, $element, options);

    if ($.isArray(tags)) {
      for (var t = 0; t < tags.length; t++) {
        var tag = tags[t];
        var item = this._normalizeItem(tag);

        var $option = this.option(item);

        this.$element.append($option);
      }
    }
  }

  Tags.prototype.query = function (decorated, params, callback) {
    var self = this;

    this._removeOldTags();

    if (params.term == null || params.page != null) {
      decorated.call(this, params, callback);
      return;
    }

    function wrapper (obj, child) {
      var data = obj.results;

      for (var i = 0; i < data.length; i++) {
        var option = data[i];

        var checkChildren = (
          option.children != null &&
          !wrapper({
            results: option.children
          }, true)
        );

        var checkText = option.text === params.term;

        if (checkText || checkChildren) {
          if (child) {
            return false;
          }

          obj.data = data;
          callback(obj);

          return;
        }
      }

      if (child) {
        return true;
      }

      var tag = self.createTag(params);

      if (tag != null) {
        var $option = self.option(tag);
        $option.attr('data-select2-tag', true);

        self.addOptions([$option]);

        self.insertTag(data, tag);
      }

      obj.results = data;

      callback(obj);
    }

    decorated.call(this, params, wrapper);
  };

  Tags.prototype.createTag = function (decorated, params) {
    var term = $.trim(params.term);

    if (term === '') {
      return null;
    }

    return {
      id: term,
      text: term
    };
  };

  Tags.prototype.insertTag = function (_, data, tag) {
    data.unshift(tag);
  };

  Tags.prototype._removeOldTags = function (_) {
    var tag = this._lastTag;

    var $options = this.$element.find('option[data-select2-tag]');

    $options.each(function () {
      if (this.selected) {
        return;
      }

      $(this).remove();
    });
  };

  return Tags;
});

S2.define('select2/data/tokenizer',[
  'jquery'
], function ($) {
  function Tokenizer (decorated, $element, options) {
    var tokenizer = options.get('tokenizer');

    if (tokenizer !== undefined) {
      this.tokenizer = tokenizer;
    }

    decorated.call(this, $element, options);
  }

  Tokenizer.prototype.bind = function (decorated, container, $container) {
    decorated.call(this, container, $container);

    this.$search =  container.dropdown.$search || container.selection.$search ||
      $container.find('.select2-search__field');
  };

  Tokenizer.prototype.query = function (decorated, params, callback) {
    var self = this;

    function select (data) {
      self.select(data);
    }

    params.term = params.term || '';

    var tokenData = this.tokenizer(params, this.options, select);

    if (tokenData.term !== params.term) {
      // Replace the search term if we have the search box
      if (this.$search.length) {
        this.$search.val(tokenData.term);
        this.$search.focus();
      }

      params.term = tokenData.term;
    }

    decorated.call(this, params, callback);
  };

  Tokenizer.prototype.tokenizer = function (_, params, options, callback) {
    var separators = options.get('tokenSeparators') || [];
    var term = params.term;
    var i = 0;

    var createTag = this.createTag || function (params) {
      return {
        id: params.term,
        text: params.term
      };
    };

    while (i < term.length) {
      var termChar = term[i];

      if ($.inArray(termChar, separators) === -1) {
        i++;

        continue;
      }

      var part = term.substr(0, i);
      var partParams = $.extend({}, params, {
        term: part
      });

      var data = createTag(partParams);

      callback(data);

      // Reset the term to not include the tokenized portion
      term = term.substr(i + 1) || '';
      i = 0;
    }

    return {
      term: term
    };
  };

  return Tokenizer;
});

S2.define('select2/data/minimumInputLength',[

], function () {
  function MinimumInputLength (decorated, $e, options) {
    this.minimumInputLength = options.get('minimumInputLength');

    decorated.call(this, $e, options);
  }

  MinimumInputLength.prototype.query = function (decorated, params, callback) {
    params.term = params.term || '';

    if (params.term.length < this.minimumInputLength) {
      this.trigger('results:message', {
        message: 'inputTooShort',
        args: {
          minimum: this.minimumInputLength,
          input: params.term,
          params: params
        }
      });

      return;
    }

    decorated.call(this, params, callback);
  };

  return MinimumInputLength;
});

S2.define('select2/data/maximumInputLength',[

], function () {
  function MaximumInputLength (decorated, $e, options) {
    this.maximumInputLength = options.get('maximumInputLength');

    decorated.call(this, $e, options);
  }

  MaximumInputLength.prototype.query = function (decorated, params, callback) {
    params.term = params.term || '';

    if (this.maximumInputLength > 0 &&
        params.term.length > this.maximumInputLength) {
      this.trigger('results:message', {
        message: 'inputTooLong',
        args: {
          maximum: this.maximumInputLength,
          input: params.term,
          params: params
        }
      });

      return;
    }

    decorated.call(this, params, callback);
  };

  return MaximumInputLength;
});

S2.define('select2/data/maximumSelectionLength',[

], function (){
  function MaximumSelectionLength (decorated, $e, options) {
    this.maximumSelectionLength = options.get('maximumSelectionLength');

    decorated.call(this, $e, options);
  }

  MaximumSelectionLength.prototype.query =
    function (decorated, params, callback) {
      var self = this;

      this.current(function (currentData) {
        var count = currentData != null ? currentData.length : 0;
        if (self.maximumSelectionLength > 0 &&
          count >= self.maximumSelectionLength) {
          self.trigger('results:message', {
            message: 'maximumSelected',
            args: {
              maximum: self.maximumSelectionLength
            }
          });
          return;
        }
        decorated.call(self, params, callback);
      });
  };

  return MaximumSelectionLength;
});

S2.define('select2/dropdown',[
  'jquery',
  './utils'
], function ($, Utils) {
  function Dropdown ($element, options) {
    this.$element = $element;
    this.options = options;

    Dropdown.__super__.constructor.call(this);
  }

  Utils.Extend(Dropdown, Utils.Observable);

  Dropdown.prototype.render = function () {
    var $dropdown = $(
      '<span class="select2-dropdown">' +
        '<span class="select2-results"></span>' +
      '</span>'
    );

    $dropdown.attr('dir', this.options.get('dir'));

    this.$dropdown = $dropdown;

    return $dropdown;
  };

  Dropdown.prototype.position = function ($dropdown, $container) {
    // Should be implmented in subclasses
  };

  Dropdown.prototype.destroy = function () {
    // Remove the dropdown from the DOM
    this.$dropdown.remove();
  };

  return Dropdown;
});

S2.define('select2/dropdown/search',[
  'jquery',
  '../utils'
], function ($, Utils) {
  function Search () { }

  Search.prototype.render = function (decorated) {
    var $rendered = decorated.call(this);

    var $search = $(
      '<span class="select2-search select2-search--dropdown">' +
        '<input class="select2-search__field" type="search" tabindex="-1"' +
        ' autocomplete="off" autocorrect="off" autocapitalize="off"' +
        ' spellcheck="false" role="textbox" />' +
      '</span>'
    );

    this.$searchContainer = $search;
    this.$search = $search.find('input');

    $rendered.prepend($search);

    return $rendered;
  };

  Search.prototype.bind = function (decorated, container, $container) {
    var self = this;

    decorated.call(this, container, $container);

    this.$search.on('keydown', function (evt) {
      self.trigger('keypress', evt);

      self._keyUpPrevented = evt.isDefaultPrevented();
    });

    // Workaround for browsers which do not support the `input` event
    // This will prevent double-triggering of events for browsers which support
    // both the `keyup` and `input` events.
    this.$search.on('input', function (evt) {
      // Unbind the duplicated `keyup` event
      $(this).off('keyup');
    });

    this.$search.on('keyup input', function (evt) {
      self.handleSearch(evt);
    });

    container.on('open', function () {
      self.$search.attr('tabindex', 0);

      self.$search.focus();

      window.setTimeout(function () {
        self.$search.focus();
      }, 0);
    });

    container.on('close', function () {
      self.$search.attr('tabindex', -1);

      self.$search.val('');
    });

    container.on('results:all', function (params) {
      if (params.query.term == null || params.query.term === '') {
        var showSearch = self.showSearch(params);

        if (showSearch) {
          self.$searchContainer.removeClass('select2-search--hide');
        } else {
          self.$searchContainer.addClass('select2-search--hide');
        }
      }
    });
  };

  Search.prototype.handleSearch = function (evt) {
    if (!this._keyUpPrevented) {
      var input = this.$search.val();

      this.trigger('query', {
        term: input
      });
    }

    this._keyUpPrevented = false;
  };

  Search.prototype.showSearch = function (_, params) {
    return true;
  };

  return Search;
});

S2.define('select2/dropdown/hidePlaceholder',[

], function () {
  function HidePlaceholder (decorated, $element, options, dataAdapter) {
    this.placeholder = this.normalizePlaceholder(options.get('placeholder'));

    decorated.call(this, $element, options, dataAdapter);
  }

  HidePlaceholder.prototype.append = function (decorated, data) {
    data.results = this.removePlaceholder(data.results);

    decorated.call(this, data);
  };

  HidePlaceholder.prototype.normalizePlaceholder = function (_, placeholder) {
    if (typeof placeholder === 'string') {
      placeholder = {
        id: '',
        text: placeholder
      };
    }

    return placeholder;
  };

  HidePlaceholder.prototype.removePlaceholder = function (_, data) {
    var modifiedData = data.slice(0);

    for (var d = data.length - 1; d >= 0; d--) {
      var item = data[d];

      if (this.placeholder.id === item.id) {
        modifiedData.splice(d, 1);
      }
    }

    return modifiedData;
  };

  return HidePlaceholder;
});

S2.define('select2/dropdown/infiniteScroll',[
  'jquery'
], function ($) {
  function InfiniteScroll (decorated, $element, options, dataAdapter) {
    this.lastParams = {};

    decorated.call(this, $element, options, dataAdapter);

    this.$loadingMore = this.createLoadingMore();
    this.loading = false;
  }

  InfiniteScroll.prototype.append = function (decorated, data) {
    this.$loadingMore.remove();
    this.loading = false;

    decorated.call(this, data);

    if (this.showLoadingMore(data)) {
      this.$results.append(this.$loadingMore);
    }
  };

  InfiniteScroll.prototype.bind = function (decorated, container, $container) {
    var self = this;

    decorated.call(this, container, $container);

    container.on('query', function (params) {
      self.lastParams = params;
      self.loading = true;
    });

    container.on('query:append', function (params) {
      self.lastParams = params;
      self.loading = true;
    });

    this.$results.on('scroll', function () {
      var isLoadMoreVisible = $.contains(
        document.documentElement,
        self.$loadingMore[0]
      );

      if (self.loading || !isLoadMoreVisible) {
        return;
      }

      var currentOffset = self.$results.offset().top +
        self.$results.outerHeight(false);
      var loadingMoreOffset = self.$loadingMore.offset().top +
        self.$loadingMore.outerHeight(false);

      if (currentOffset + 50 >= loadingMoreOffset) {
        self.loadMore();
      }
    });
  };

  InfiniteScroll.prototype.loadMore = function () {
    this.loading = true;

    var params = $.extend({}, {page: 1}, this.lastParams);

    params.page++;

    this.trigger('query:append', params);
  };

  InfiniteScroll.prototype.showLoadingMore = function (_, data) {
    return data.pagination && data.pagination.more;
  };

  InfiniteScroll.prototype.createLoadingMore = function () {
    var $option = $(
      '<li class="option load-more" role="treeitem"></li>'
    );

    var message = this.options.get('translations').get('loadingMore');

    $option.html(message(this.lastParams));

    return $option;
  };

  return InfiniteScroll;
});

S2.define('select2/dropdown/attachBody',[
  'jquery',
  '../utils'
], function ($, Utils) {
  function AttachBody (decorated, $element, options) {
    this.$dropdownParent = options.get('dropdownParent') || document.body;

    decorated.call(this, $element, options);
  }

  AttachBody.prototype.bind = function (decorated, container, $container) {
    var self = this;

    var setupResultsEvents = false;

    decorated.call(this, container, $container);

    container.on('open', function () {
      self._showDropdown();
      self._attachPositioningHandler(container);

      if (!setupResultsEvents) {
        setupResultsEvents = true;

        container.on('results:all', function () {
          self._positionDropdown();
          self._resizeDropdown();
        });

        container.on('results:append', function () {
          self._positionDropdown();
          self._resizeDropdown();
        });
      }
    });

    container.on('close', function () {
      self._hideDropdown();
      self._detachPositioningHandler(container);
    });

    this.$dropdownContainer.on('mousedown', function (evt) {
      evt.stopPropagation();
    });
  };

  AttachBody.prototype.position = function (decorated, $dropdown, $container) {
    // Clone all of the container classes
    $dropdown.attr('class', $container.attr('class'));

    $dropdown.removeClass('select2');
    $dropdown.addClass('select2-container--open');

    $dropdown.css({
      position: 'absolute',
      top: -999999
    });

    this.$container = $container;
  };

  AttachBody.prototype.render = function (decorated) {
    var $container = $('<span></span>');

    var $dropdown = decorated.call(this);
    $container.append($dropdown);

    this.$dropdownContainer = $container;

    return $container;
  };

  AttachBody.prototype._hideDropdown = function (decorated) {
    this.$dropdownContainer.detach();
  };

  AttachBody.prototype._attachPositioningHandler = function (container) {
    var self = this;

    var scrollEvent = 'scroll.select2.' + container.id;
    var resizeEvent = 'resize.select2.' + container.id;
    var orientationEvent = 'orientationchange.select2.' + container.id;

    var $watchers = this.$container.parents().filter(Utils.hasScroll);
    $watchers.each(function () {
      $(this).data('select2-scroll-position', {
        x: $(this).scrollLeft(),
        y: $(this).scrollTop()
      });
    });

    $watchers.on(scrollEvent, function (ev) {
      var position = $(this).data('select2-scroll-position');
      $(this).scrollTop(position.y);
    });

    $(window).on(scrollEvent + ' ' + resizeEvent + ' ' + orientationEvent,
      function (e) {
      self._positionDropdown();
      self._resizeDropdown();
    });
  };

  AttachBody.prototype._detachPositioningHandler = function (container) {
    var scrollEvent = 'scroll.select2.' + container.id;
    var resizeEvent = 'resize.select2.' + container.id;
    var orientationEvent = 'orientationchange.select2.' + container.id;

    var $watchers = this.$container.parents().filter(Utils.hasScroll);
    $watchers.off(scrollEvent);

    $(window).off(scrollEvent + ' ' + resizeEvent + ' ' + orientationEvent);
  };

  AttachBody.prototype._positionDropdown = function () {
    var $window = $(window);

    var isCurrentlyAbove = this.$dropdown.hasClass('select2-dropdown--above');
    var isCurrentlyBelow = this.$dropdown.hasClass('select2-dropdown--below');

    var newDirection = null;

    var position = this.$container.position();
    var offset = this.$container.offset();

    offset.bottom = offset.top + this.$container.outerHeight(false);

    var container = {
      height: this.$container.outerHeight(false)
    };

    container.top = offset.top;
    container.bottom = offset.top + container.height;

    var dropdown = {
      height: this.$dropdown.outerHeight(false)
    };

    var viewport = {
      top: $window.scrollTop(),
      bottom: $window.scrollTop() + $window.height()
    };

    var enoughRoomAbove = viewport.top < (offset.top - dropdown.height);
    var enoughRoomBelow = viewport.bottom > (offset.bottom + dropdown.height);

    var css = {
      left: offset.left,
      top: container.bottom
    };

    if (!isCurrentlyAbove && !isCurrentlyBelow) {
      newDirection = 'below';
    }

    if (!enoughRoomBelow && enoughRoomAbove && !isCurrentlyAbove) {
      newDirection = 'above';
    } else if (!enoughRoomAbove && enoughRoomBelow && isCurrentlyAbove) {
      newDirection = 'below';
    }

    if (newDirection == 'above' ||
      (isCurrentlyAbove && newDirection !== 'below')) {
      css.top = container.top - dropdown.height;
    }

    if (newDirection != null) {
      this.$dropdown
        .removeClass('select2-dropdown--below select2-dropdown--above')
        .addClass('select2-dropdown--' + newDirection);
      this.$container
        .removeClass('select2-container--below select2-container--above')
        .addClass('select2-container--' + newDirection);
    }

    this.$dropdownContainer.css(css);
  };

  AttachBody.prototype._resizeDropdown = function () {
    this.$dropdownContainer.width();

    var css = {
      width: this.$container.outerWidth(false) + 'px'
    };

    if (this.options.get('dropdownAutoWidth')) {
      css.minWidth = css.width;
      css.width = 'auto';
    }

    this.$dropdown.css(css);
  };

  AttachBody.prototype._showDropdown = function (decorated) {
    this.$dropdownContainer.appendTo(this.$dropdownParent);

    this._positionDropdown();
    this._resizeDropdown();
  };

  return AttachBody;
});

S2.define('select2/dropdown/minimumResultsForSearch',[

], function () {
  function countResults (data) {
    var count = 0;

    for (var d = 0; d < data.length; d++) {
      var item = data[d];

      if (item.children) {
        count += countResults(item.children);
      } else {
        count++;
      }
    }

    return count;
  }

  function MinimumResultsForSearch (decorated, $element, options, dataAdapter) {
    this.minimumResultsForSearch = options.get('minimumResultsForSearch');

    if (this.minimumResultsForSearch < 0) {
      this.minimumResultsForSearch = Infinity;
    }

    decorated.call(this, $element, options, dataAdapter);
  }

  MinimumResultsForSearch.prototype.showSearch = function (decorated, params) {
    if (countResults(params.data.results) < this.minimumResultsForSearch) {
      return false;
    }

    return decorated.call(this, params);
  };

  return MinimumResultsForSearch;
});

S2.define('select2/dropdown/selectOnClose',[

], function () {
  function SelectOnClose () { }

  SelectOnClose.prototype.bind = function (decorated, container, $container) {
    var self = this;

    decorated.call(this, container, $container);

    container.on('close', function () {
      self._handleSelectOnClose();
    });
  };

  SelectOnClose.prototype._handleSelectOnClose = function () {
    var $highlightedResults = this.getHighlightedResults();

    if ($highlightedResults.length < 1) {
      return;
    }

    this.trigger('select', {
        data: $highlightedResults.data('data')
    });
  };

  return SelectOnClose;
});

S2.define('select2/dropdown/closeOnSelect',[

], function () {
  function CloseOnSelect () { }

  CloseOnSelect.prototype.bind = function (decorated, container, $container) {
    var self = this;

    decorated.call(this, container, $container);

    container.on('select', function (evt) {
      self._selectTriggered(evt);
    });

    container.on('unselect', function (evt) {
      self._selectTriggered(evt);
    });
  };

  CloseOnSelect.prototype._selectTriggered = function (_, evt) {
    var originalEvent = evt.originalEvent;

    // Don't close if the control key is being held
    if (originalEvent && originalEvent.ctrlKey) {
      return;
    }

    this.trigger('close');
  };

  return CloseOnSelect;
});

S2.define('select2/i18n/en',[],function () {
  // English
  return {
    errorLoading: function () {
      return 'The results could not be loaded.';
    },
    inputTooLong: function (args) {
      var overChars = args.input.length - args.maximum;

      var message = 'Please delete ' + overChars + ' character';

      if (overChars != 1) {
        message += 's';
      }

      return message;
    },
    inputTooShort: function (args) {
      var remainingChars = args.minimum - args.input.length;

      var message = 'Please enter ' + remainingChars + ' or more characters';

      return message;
    },
    loadingMore: function () {
      return 'Loading more results…';
    },
    maximumSelected: function (args) {
      var message = 'You can only select ' + args.maximum + ' item';

      if (args.maximum != 1) {
        message += 's';
      }

      return message;
    },
    noResults: function () {
      return 'No results found';
    },
    searching: function () {
      return 'Searching…';
    }
  };
});

S2.define('select2/defaults',[
  'jquery',
  'require',

  './results',

  './selection/single',
  './selection/multiple',
  './selection/placeholder',
  './selection/allowClear',
  './selection/search',
  './selection/eventRelay',

  './utils',
  './translation',
  './diacritics',

  './data/select',
  './data/array',
  './data/ajax',
  './data/tags',
  './data/tokenizer',
  './data/minimumInputLength',
  './data/maximumInputLength',
  './data/maximumSelectionLength',

  './dropdown',
  './dropdown/search',
  './dropdown/hidePlaceholder',
  './dropdown/infiniteScroll',
  './dropdown/attachBody',
  './dropdown/minimumResultsForSearch',
  './dropdown/selectOnClose',
  './dropdown/closeOnSelect',

  './i18n/en'
], function ($, require,

             ResultsList,

             SingleSelection, MultipleSelection, Placeholder, AllowClear,
             SelectionSearch, EventRelay,

             Utils, Translation, DIACRITICS,

             SelectData, ArrayData, AjaxData, Tags, Tokenizer,
             MinimumInputLength, MaximumInputLength, MaximumSelectionLength,

             Dropdown, DropdownSearch, HidePlaceholder, InfiniteScroll,
             AttachBody, MinimumResultsForSearch, SelectOnClose, CloseOnSelect,

             EnglishTranslation) {
  function Defaults () {
    this.reset();
  }

  Defaults.prototype.apply = function (options) {
    options = $.extend({}, this.defaults, options);

    if (options.dataAdapter == null) {
      if (options.ajax != null) {
        options.dataAdapter = AjaxData;
      } else if (options.data != null) {
        options.dataAdapter = ArrayData;
      } else {
        options.dataAdapter = SelectData;
      }

      if (options.minimumInputLength > 0) {
        options.dataAdapter = Utils.Decorate(
          options.dataAdapter,
          MinimumInputLength
        );
      }

      if (options.maximumInputLength > 0) {
        options.dataAdapter = Utils.Decorate(
          options.dataAdapter,
          MaximumInputLength
        );
      }

      if (options.maximumSelectionLength > 0) {
        options.dataAdapter = Utils.Decorate(
          options.dataAdapter,
          MaximumSelectionLength
        );
      }

      if (options.tags) {
        options.dataAdapter = Utils.Decorate(options.dataAdapter, Tags);
      }

      if (options.tokenSeparators != null || options.tokenizer != null) {
        options.dataAdapter = Utils.Decorate(
          options.dataAdapter,
          Tokenizer
        );
      }

      if (options.query != null) {
        var Query = require(options.amdBase + 'compat/query');

        options.dataAdapter = Utils.Decorate(
          options.dataAdapter,
          Query
        );
      }

      if (options.initSelection != null) {
        var InitSelection = require(options.amdBase + 'compat/initSelection');

        options.dataAdapter = Utils.Decorate(
          options.dataAdapter,
          InitSelection
        );
      }
    }

    if (options.resultsAdapter == null) {
      options.resultsAdapter = ResultsList;

      if (options.ajax != null) {
        options.resultsAdapter = Utils.Decorate(
          options.resultsAdapter,
          InfiniteScroll
        );
      }

      if (options.placeholder != null) {
        options.resultsAdapter = Utils.Decorate(
          options.resultsAdapter,
          HidePlaceholder
        );
      }

      if (options.selectOnClose) {
        options.resultsAdapter = Utils.Decorate(
          options.resultsAdapter,
          SelectOnClose
        );
      }
    }

    if (options.dropdownAdapter == null) {
      if (options.multiple) {
        options.dropdownAdapter = Dropdown;
      } else {
        var SearchableDropdown = Utils.Decorate(Dropdown, DropdownSearch);

        options.dropdownAdapter = SearchableDropdown;
      }

      if (options.minimumResultsForSearch !== 0) {
        options.dropdownAdapter = Utils.Decorate(
          options.dropdownAdapter,
          MinimumResultsForSearch
        );
      }

      if (options.closeOnSelect) {
        options.dropdownAdapter = Utils.Decorate(
          options.dropdownAdapter,
          CloseOnSelect
        );
      }

      if (
        options.dropdownCssClass != null ||
        options.dropdownCss != null ||
        options.adaptDropdownCssClass != null
      ) {
        var DropdownCSS = require(options.amdBase + 'compat/dropdownCss');

        options.dropdownAdapter = Utils.Decorate(
          options.dropdownAdapter,
          DropdownCSS
        );
      }

      options.dropdownAdapter = Utils.Decorate(
        options.dropdownAdapter,
        AttachBody
      );
    }

    if (options.selectionAdapter == null) {
      if (options.multiple) {
        options.selectionAdapter = MultipleSelection;
      } else {
        options.selectionAdapter = SingleSelection;
      }

      // Add the placeholder mixin if a placeholder was specified
      if (options.placeholder != null) {
        options.selectionAdapter = Utils.Decorate(
          options.selectionAdapter,
          Placeholder
        );
      }

      if (options.allowClear) {
        options.selectionAdapter = Utils.Decorate(
          options.selectionAdapter,
          AllowClear
        );
      }

      if (options.multiple) {
        options.selectionAdapter = Utils.Decorate(
          options.selectionAdapter,
          SelectionSearch
        );
      }

      if (
        options.containerCssClass != null ||
        options.containerCss != null ||
        options.adaptContainerCssClass != null
      ) {
        var ContainerCSS = require(options.amdBase + 'compat/containerCss');

        options.selectionAdapter = Utils.Decorate(
          options.selectionAdapter,
          ContainerCSS
        );
      }

      options.selectionAdapter = Utils.Decorate(
        options.selectionAdapter,
        EventRelay
      );
    }

    if (typeof options.language === 'string') {
      // Check if the language is specified with a region
      if (options.language.indexOf('-') > 0) {
        // Extract the region information if it is included
        var languageParts = options.language.split('-');
        var baseLanguage = languageParts[0];

        options.language = [options.language, baseLanguage];
      } else {
        options.language = [options.language];
      }
    }

    if ($.isArray(options.language)) {
      var languages = new Translation();
      options.language.push('en');

      var languageNames = options.language;

      for (var l = 0; l < languageNames.length; l++) {
        var name = languageNames[l];
        var language = {};

        try {
          // Try to load it with the original name
          language = Translation.loadPath(name);
        } catch (e) {
          try {
            // If we couldn't load it, check if it wasn't the full path
            name = this.defaults.amdLanguageBase + name;
            language = Translation.loadPath(name);
          } catch (ex) {
            // The translation could not be loaded at all. Sometimes this is
            // because of a configuration problem, other times this can be
            // because of how Select2 helps load all possible translation files.
            if (options.debug && window.console && console.warn) {
              console.warn(
                'Select2: The language file for "' + name + '" could not be ' +
                'automatically loaded. A fallback will be used instead.'
              );
            }

            continue;
          }
        }

        languages.extend(language);
      }

      options.translations = languages;
    } else {
      var baseTranslation = Translation.loadPath(
        this.defaults.amdLanguageBase + 'en'
      );
      var customTranslation = new Translation(options.language);

      customTranslation.extend(baseTranslation);

      options.translations = customTranslation;
    }

    return options;
  };

  Defaults.prototype.reset = function () {
    function stripDiacritics (text) {
      // Used 'uni range + named function' from http://jsperf.com/diacritics/18
      function match(a) {
        return DIACRITICS[a] || a;
      }

      return text.replace(/[^\u0000-\u007E]/g, match);
    }

    function matcher (params, data) {
      // Always return the object if there is nothing to compare
      if ($.trim(params.term) === '') {
        return data;
      }

      // Do a recursive check for options with children
      if (data.children && data.children.length > 0) {
        // Clone the data object if there are children
        // This is required as we modify the object to remove any non-matches
        var match = $.extend(true, {}, data);

        // Check each child of the option
        for (var c = data.children.length - 1; c >= 0; c--) {
          var child = data.children[c];

          var matches = matcher(params, child);

          // If there wasn't a match, remove the object in the array
          if (matches == null) {
            match.children.splice(c, 1);
          }
        }

        // If any children matched, return the new object
        if (match.children.length > 0) {
          return match;
        }

        // If there were no matching children, check just the plain object
        return matcher(params, match);
      }

      var original = stripDiacritics(data.text).toUpperCase();
      var term = stripDiacritics(params.term).toUpperCase();

      // Check if the text contains the term
      if (original.indexOf(term) > -1) {
        return data;
      }

      // If it doesn't contain the term, don't return anything
      return null;
    }

    this.defaults = {
      amdBase: './',
      amdLanguageBase: './i18n/',
      closeOnSelect: true,
      debug: false,
      dropdownAutoWidth: false,
      escapeMarkup: Utils.escapeMarkup,
      language: EnglishTranslation,
      matcher: matcher,
      minimumInputLength: 0,
      maximumInputLength: 0,
      maximumSelectionLength: 0,
      minimumResultsForSearch: 0,
      selectOnClose: false,
      sorter: function (data) {
        return data;
      },
      templateResult: function (result) {
        return result.text;
      },
      templateSelection: function (selection) {
        return selection.text;
      },
      theme: 'default',
      width: 'resolve'
    };
  };

  Defaults.prototype.set = function (key, value) {
    var camelKey = $.camelCase(key);

    var data = {};
    data[camelKey] = value;

    var convertedData = Utils._convertData(data);

    $.extend(this.defaults, convertedData);
  };

  var defaults = new Defaults();

  return defaults;
});

S2.define('select2/options',[
  'require',
  'jquery',
  './defaults',
  './utils'
], function (require, $, Defaults, Utils) {
  function Options (options, $element) {
    this.options = options;

    if ($element != null) {
      this.fromElement($element);
    }

    this.options = Defaults.apply(this.options);

    if ($element && $element.is('input')) {
      var InputCompat = require(this.get('amdBase') + 'compat/inputData');

      this.options.dataAdapter = Utils.Decorate(
        this.options.dataAdapter,
        InputCompat
      );
    }
  }

  Options.prototype.fromElement = function ($e) {
    var excludedData = ['select2'];

    if (this.options.multiple == null) {
      this.options.multiple = $e.prop('multiple');
    }

    if (this.options.disabled == null) {
      this.options.disabled = $e.prop('disabled');
    }

    if (this.options.language == null) {
      if ($e.prop('lang')) {
        this.options.language = $e.prop('lang').toLowerCase();
      } else if ($e.closest('[lang]').prop('lang')) {
        this.options.language = $e.closest('[lang]').prop('lang');
      }
    }

    if (this.options.dir == null) {
      if ($e.prop('dir')) {
        this.options.dir = $e.prop('dir');
      } else if ($e.closest('[dir]').prop('dir')) {
        this.options.dir = $e.closest('[dir]').prop('dir');
      } else {
        this.options.dir = 'ltr';
      }
    }

    $e.prop('disabled', this.options.disabled);
    $e.prop('multiple', this.options.multiple);

    if ($e.data('select2Tags')) {
      if (this.options.debug && window.console && console.warn) {
        console.warn(
          'Select2: The `data-select2-tags` attribute has been changed to ' +
          'use the `data-data` and `data-tags="true"` attributes and will be ' +
          'removed in future versions of Select2.'
        );
      }

      $e.data('data', $e.data('select2Tags'));
      $e.data('tags', true);
    }

    if ($e.data('ajaxUrl')) {
      if (this.options.debug && window.console && console.warn) {
        console.warn(
          'Select2: The `data-ajax-url` attribute has been changed to ' +
          '`data-ajax--url` and support for the old attribute will be removed' +
          ' in future versions of Select2.'
        );
      }

      $e.attr('ajax--url', $e.data('ajaxUrl'));
      $e.data('ajax--url', $e.data('ajaxUrl'));
    }

    var dataset = {};

    // Prefer the element's `dataset` attribute if it exists
    // jQuery 1.x does not correctly handle data attributes with multiple dashes
    if ($.fn.jquery && $.fn.jquery.substr(0, 2) == '1.' && $e[0].dataset) {
      dataset = $.extend(true, {}, $e[0].dataset, $e.data());
    } else {
      dataset = $e.data();
    }

    var data = $.extend(true, {}, dataset);

    data = Utils._convertData(data);

    for (var key in data) {
      if ($.inArray(key, excludedData) > -1) {
        continue;
      }

      if ($.isPlainObject(this.options[key])) {
        $.extend(this.options[key], data[key]);
      } else {
        this.options[key] = data[key];
      }
    }

    return this;
  };

  Options.prototype.get = function (key) {
    return this.options[key];
  };

  Options.prototype.set = function (key, val) {
    this.options[key] = val;
  };

  return Options;
});

S2.define('select2/core',[
  'jquery',
  './options',
  './utils',
  './keys'
], function ($, Options, Utils, KEYS) {
  var Select2 = function ($element, options) {
    if ($element.data('select2') != null) {
      $element.data('select2').destroy();
    }

    this.$element = $element;

    this.id = this._generateId($element);

    options = options || {};

    this.options = new Options(options, $element);

    Select2.__super__.constructor.call(this);

    // Set up the tabindex

    var tabindex = $element.attr('tabindex') || 0;
    $element.data('old-tabindex', tabindex);
    $element.attr('tabindex', '-1');

    // Set up containers and adapters

    var DataAdapter = this.options.get('dataAdapter');
    this.dataAdapter = new DataAdapter($element, this.options);

    var $container = this.render();

    this._placeContainer($container);

    var SelectionAdapter = this.options.get('selectionAdapter');
    this.selection = new SelectionAdapter($element, this.options);
    this.$selection = this.selection.render();

    this.selection.position(this.$selection, $container);

    var DropdownAdapter = this.options.get('dropdownAdapter');
    this.dropdown = new DropdownAdapter($element, this.options);
    this.$dropdown = this.dropdown.render();

    this.dropdown.position(this.$dropdown, $container);

    var ResultsAdapter = this.options.get('resultsAdapter');
    this.results = new ResultsAdapter($element, this.options, this.dataAdapter);
    this.$results = this.results.render();

    this.results.position(this.$results, this.$dropdown);

    // Bind events

    var self = this;

    // Bind the container to all of the adapters
    this._bindAdapters();

    // Register any DOM event handlers
    this._registerDomEvents();

    // Register any internal event handlers
    this._registerDataEvents();
    this._registerSelectionEvents();
    this._registerDropdownEvents();
    this._registerResultsEvents();
    this._registerEvents();

    // Set the initial state
    this.dataAdapter.current(function (initialData) {
      self.trigger('selection:update', {
        data: initialData
      });
    });

    // Hide the original select
    $element.addClass('select2-hidden-accessible');
	$element.attr('aria-hidden', 'true');
	
    // Synchronize any monitored attributes
    this._syncAttributes();

    $element.data('select2', this);
  };

  Utils.Extend(Select2, Utils.Observable);

  Select2.prototype._generateId = function ($element) {
    var id = '';

    if ($element.attr('id') != null) {
      id = $element.attr('id');
    } else if ($element.attr('name') != null) {
      id = $element.attr('name') + '-' + Utils.generateChars(2);
    } else {
      id = Utils.generateChars(4);
    }

    id = 'select2-' + id;

    return id;
  };

  Select2.prototype._placeContainer = function ($container) {
    $container.insertAfter(this.$element);

    var width = this._resolveWidth(this.$element, this.options.get('width'));

    if (width != null) {
      $container.css('width', width);
    }
  };

  Select2.prototype._resolveWidth = function ($element, method) {
    var WIDTH = /^width:(([-+]?([0-9]*\.)?[0-9]+)(px|em|ex|%|in|cm|mm|pt|pc))/i;

    if (method == 'resolve') {
      var styleWidth = this._resolveWidth($element, 'style');

      if (styleWidth != null) {
        return styleWidth;
      }

      return this._resolveWidth($element, 'element');
    }

    if (method == 'element') {
      var elementWidth = $element.outerWidth(false);

      if (elementWidth <= 0) {
        return 'auto';
      }

      return elementWidth + 'px';
    }

    if (method == 'style') {
      var style = $element.attr('style');

      if (typeof(style) !== 'string') {
        return null;
      }

      var attrs = style.split(';');

      for (var i = 0, l = attrs.length; i < l; i = i + 1) {
        var attr = attrs[i].replace(/\s/g, '');
        var matches = attr.match(WIDTH);

        if (matches !== null && matches.length >= 1) {
          return matches[1];
        }
      }

      return null;
    }

    return method;
  };

  Select2.prototype._bindAdapters = function () {
    this.dataAdapter.bind(this, this.$container);
    this.selection.bind(this, this.$container);

    this.dropdown.bind(this, this.$container);
    this.results.bind(this, this.$container);
  };

  Select2.prototype._registerDomEvents = function () {
    var self = this;

    this.$element.on('change.select2', function () {
      self.dataAdapter.current(function (data) {
        self.trigger('selection:update', {
          data: data
        });
      });
    });

    this._sync = Utils.bind(this._syncAttributes, this);

    if (this.$element[0].attachEvent) {
      this.$element[0].attachEvent('onpropertychange', this._sync);
    }

    var observer = window.MutationObserver ||
      window.WebKitMutationObserver ||
      window.MozMutationObserver
    ;

    if (observer != null) {
      this._observer = new observer(function (mutations) {
        $.each(mutations, self._sync);
      });
      this._observer.observe(this.$element[0], {
        attributes: true,
        subtree: false
      });
    } else if (this.$element[0].addEventListener) {
      this.$element[0].addEventListener('DOMAttrModified', self._sync, false);
    }
  };

  Select2.prototype._registerDataEvents = function () {
    var self = this;

    this.dataAdapter.on('*', function (name, params) {
      self.trigger(name, params);
    });
  };

  Select2.prototype._registerSelectionEvents = function () {
    var self = this;
    var nonRelayEvents = ['toggle'];

    this.selection.on('toggle', function () {
      self.toggleDropdown();
    });

    this.selection.on('*', function (name, params) {
      if ($.inArray(name, nonRelayEvents) !== -1) {
        return;
      }

      self.trigger(name, params);
    });
  };

  Select2.prototype._registerDropdownEvents = function () {
    var self = this;

    this.dropdown.on('*', function (name, params) {
      self.trigger(name, params);
    });
  };

  Select2.prototype._registerResultsEvents = function () {
    var self = this;

    this.results.on('*', function (name, params) {
      self.trigger(name, params);
    });
  };

  Select2.prototype._registerEvents = function () {
    var self = this;

    this.on('open', function () {
      self.$container.addClass('select2-container--open');
    });

    this.on('close', function () {
      self.$container.removeClass('select2-container--open');
    });

    this.on('enable', function () {
      self.$container.removeClass('select2-container--disabled');
    });

    this.on('disable', function () {
      self.$container.addClass('select2-container--disabled');
    });

    this.on('focus', function () {
      self.$container.addClass('select2-container--focus');
    });

    this.on('blur', function () {
      self.$container.removeClass('select2-container--focus');
    });

    this.on('query', function (params) {
      if (!self.isOpen()) {
        self.trigger('open');
      }

      this.dataAdapter.query(params, function (data) {
        self.trigger('results:all', {
          data: data,
          query: params
        });
      });
    });

    this.on('query:append', function (params) {
      this.dataAdapter.query(params, function (data) {
        self.trigger('results:append', {
          data: data,
          query: params
        });
      });
    });

    this.on('keypress', function (evt) {
      var key = evt.which;

      if (self.isOpen()) {
        if (key === KEYS.ENTER) {
          self.trigger('results:select');

          evt.preventDefault();
        } else if ((key === KEYS.SPACE && evt.ctrlKey)) {
          self.trigger('results:toggle');

          evt.preventDefault();
        } else if (key === KEYS.UP) {
          self.trigger('results:previous');

          evt.preventDefault();
        } else if (key === KEYS.DOWN) {
          self.trigger('results:next');

          evt.preventDefault();
        } else if (key === KEYS.ESC || key === KEYS.TAB) {
          self.close();

          evt.preventDefault();
        }
      } else {
        if (key === KEYS.ENTER || key === KEYS.SPACE ||
            ((key === KEYS.DOWN || key === KEYS.UP) && evt.altKey)) {
          self.open();

          evt.preventDefault();
        }
      }
    });
  };

  Select2.prototype._syncAttributes = function () {
    this.options.set('disabled', this.$element.prop('disabled'));

    if (this.options.get('disabled')) {
      if (this.isOpen()) {
        this.close();
      }

      this.trigger('disable');
    } else {
      this.trigger('enable');
    }
  };

  /**
   * Override the trigger method to automatically trigger pre-events when
   * there are events that can be prevented.
   */
  Select2.prototype.trigger = function (name, args) {
    var actualTrigger = Select2.__super__.trigger;
    var preTriggerMap = {
      'open': 'opening',
      'close': 'closing',
      'select': 'selecting',
      'unselect': 'unselecting'
    };

    if (name in preTriggerMap) {
      var preTriggerName = preTriggerMap[name];
      var preTriggerArgs = {
        prevented: false,
        name: name,
        args: args
      };

      actualTrigger.call(this, preTriggerName, preTriggerArgs);

      if (preTriggerArgs.prevented) {
        args.prevented = true;

        return;
      }
    }

    actualTrigger.call(this, name, args);
  };

  Select2.prototype.toggleDropdown = function () {
    if (this.options.get('disabled')) {
      return;
    }

    if (this.isOpen()) {
      this.close();
    } else {
      this.open();
    }
  };

  Select2.prototype.open = function () {
    if (this.isOpen()) {
      return;
    }

    this.trigger('query', {});

    this.trigger('open');
  };

  Select2.prototype.close = function () {
    if (!this.isOpen()) {
      return;
    }

    this.trigger('close');
  };

  Select2.prototype.isOpen = function () {
    return this.$container.hasClass('select2-container--open');
  };

  Select2.prototype.enable = function (args) {
    if (this.options.get('debug') && window.console && console.warn) {
      console.warn(
        'Select2: The `select2("enable")` method has been deprecated and will' +
        ' be removed in later Select2 versions. Use $element.prop("disabled")' +
        ' instead.'
      );
    }

    if (args == null || args.length === 0) {
      args = [true];
    }

    var disabled = !args[0];

    this.$element.prop('disabled', disabled);
  };

  Select2.prototype.data = function () {
    if (this.options.get('debug') &&
        arguments.length > 0 && window.console && console.warn) {
      console.warn(
        'Select2: Data can no longer be set using `select2("data")`. You ' +
        'should consider setting the value instead using `$element.val()`.'
      );
    }

    var data = [];

    this.dataAdapter.current(function (currentData) {
      data = currentData;
    });

    return data;
  };

  Select2.prototype.val = function (args) {
    if (this.options.get('debug') && window.console && console.warn) {
      console.warn(
        'Select2: The `select2("val")` method has been deprecated and will be' +
        ' removed in later Select2 versions. Use $element.val() instead.'
      );
    }

    if (args == null || args.length === 0) {
      return this.$element.val();
    }

    var newVal = args[0];

    if ($.isArray(newVal)) {
      newVal = $.map(newVal, function (obj) {
        return obj.toString();
      });
    }

    this.$element.val(newVal).trigger('change');
  };

  Select2.prototype.destroy = function () {
    this.$container.remove();

    if (this.$element[0].detachEvent) {
      this.$element[0].detachEvent('onpropertychange', this._sync);
    }

    if (this._observer != null) {
      this._observer.disconnect();
      this._observer = null;
    } else if (this.$element[0].removeEventListener) {
      this.$element[0]
        .removeEventListener('DOMAttrModified', this._sync, false);
    }

    this._sync = null;

    this.$element.off('.select2');
    this.$element.attr('tabindex', this.$element.data('old-tabindex'));

    this.$element.removeClass('select2-hidden-accessible');
	this.$element.attr('aria-hidden', 'false');
    this.$element.removeData('select2');

    this.dataAdapter.destroy();
    this.selection.destroy();
    this.dropdown.destroy();
    this.results.destroy();

    this.dataAdapter = null;
    this.selection = null;
    this.dropdown = null;
    this.results = null;
  };

  Select2.prototype.render = function () {
    var $container = $(
      '<span class="select2 select2-container">' +
        '<span class="selection"></span>' +
        '<span class="dropdown-wrapper" aria-hidden="true"></span>' +
      '</span>'
    );

    $container.attr('dir', this.options.get('dir'));

    this.$container = $container;

    this.$container.addClass('select2-container--' + this.options.get('theme'));

    $container.data('element', this.$element);

    return $container;
  };

  return Select2;
});

S2.define('select2/compat/utils',[
  'jquery'
], function ($) {
  function syncCssClasses ($dest, $src, adapter) {
    var classes, replacements = [], adapted;

    classes = $.trim($dest.attr('class'));

    if (classes) {
      classes = '' + classes; // for IE which returns object

      $(classes.split(/\s+/)).each(function () {
        // Save all Select2 classes
        if (this.indexOf('select2-') === 0) {
          replacements.push(this);
        }
      });
    }

    classes = $.trim($src.attr('class'));

    if (classes) {
      classes = '' + classes; // for IE which returns object

      $(classes.split(/\s+/)).each(function () {
        // Only adapt non-Select2 classes
        if (this.indexOf('select2-') !== 0) {
          adapted = adapter(this);

          if (adapted != null) {
            replacements.push(adapted);
          }
        }
      });
    }

    $dest.attr('class', replacements.join(' '));
  }

  return {
    syncCssClasses: syncCssClasses
  };
});

S2.define('select2/compat/containerCss',[
  'jquery',
  './utils'
], function ($, CompatUtils) {
  // No-op CSS adapter that discards all classes by default
  function _containerAdapter (clazz) {
    return null;
  }

  function ContainerCSS () { }

  ContainerCSS.prototype.render = function (decorated) {
    var $container = decorated.call(this);

    var containerCssClass = this.options.get('containerCssClass') || '';

    if ($.isFunction(containerCssClass)) {
      containerCssClass = containerCssClass(this.$element);
    }

    var containerCssAdapter = this.options.get('adaptContainerCssClass');
    containerCssAdapter = containerCssAdapter || _containerAdapter;

    if (containerCssClass.indexOf(':all:') !== -1) {
      containerCssClass = containerCssClass.replace(':all', '');

      var _cssAdapter = containerCssAdapter;

      containerCssAdapter = function (clazz) {
        var adapted = _cssAdapter(clazz);

        if (adapted != null) {
          // Append the old one along with the adapted one
          return adapted + ' ' + clazz;
        }

        return clazz;
      };
    }

    var containerCss = this.options.get('containerCss') || {};

    if ($.isFunction(containerCss)) {
      containerCss = containerCss(this.$element);
    }

    CompatUtils.syncCssClasses($container, this.$element, containerCssAdapter);

    $container.css(containerCss);
    $container.addClass(containerCssClass);

    return $container;
  };

  return ContainerCSS;
});

S2.define('select2/compat/dropdownCss',[
  'jquery',
  './utils'
], function ($, CompatUtils) {
  // No-op CSS adapter that discards all classes by default
  function _dropdownAdapter (clazz) {
    return null;
  }

  function DropdownCSS () { }

  DropdownCSS.prototype.render = function (decorated) {
    var $dropdown = decorated.call(this);

    var dropdownCssClass = this.options.get('dropdownCssClass') || '';

    if ($.isFunction(dropdownCssClass)) {
      dropdownCssClass = dropdownCssClass(this.$element);
    }

    var dropdownCssAdapter = this.options.get('adaptDropdownCssClass');
    dropdownCssAdapter = dropdownCssAdapter || _dropdownAdapter;

    if (dropdownCssClass.indexOf(':all:') !== -1) {
      dropdownCssClass = dropdownCssClass.replace(':all', '');

      var _cssAdapter = dropdownCssAdapter;

      dropdownCssAdapter = function (clazz) {
        var adapted = _cssAdapter(clazz);

        if (adapted != null) {
          // Append the old one along with the adapted one
          return adapted + ' ' + clazz;
        }

        return clazz;
      };
    }

    var dropdownCss = this.options.get('dropdownCss') || {};

    if ($.isFunction(dropdownCss)) {
      dropdownCss = dropdownCss(this.$element);
    }

    CompatUtils.syncCssClasses($dropdown, this.$element, dropdownCssAdapter);

    $dropdown.css(dropdownCss);
    $dropdown.addClass(dropdownCssClass);

    return $dropdown;
  };

  return DropdownCSS;
});

S2.define('select2/compat/initSelection',[
  'jquery'
], function ($) {
  function InitSelection (decorated, $element, options) {
    if (options.get('debug') && window.console && console.warn) {
      console.warn(
        'Select2: The `initSelection` option has been deprecated in favor' +
        ' of a custom data adapter that overrides the `current` method. ' +
        'This method is now called multiple times instead of a single ' +
        'time when the instance is initialized. Support will be removed ' +
        'for the `initSelection` option in future versions of Select2'
      );
    }

    this.initSelection = options.get('initSelection');
    this._isInitialized = false;

    decorated.call(this, $element, options);
  }

  InitSelection.prototype.current = function (decorated, callback) {
    var self = this;

    if (this._isInitialized) {
      decorated.call(this, callback);

      return;
    }

    this.initSelection.call(null, this.$element, function (data) {
      self._isInitialized = true;

      if (!$.isArray(data)) {
        data = [data];
      }

      callback(data);
    });
  };

  return InitSelection;
});

S2.define('select2/compat/inputData',[
  'jquery'
], function ($) {
  function InputData (decorated, $element, options) {
    this._currentData = [];
    this._valueSeparator = options.get('valueSeparator') || ',';

    if ($element.prop('type') === 'hidden') {
      if (options.get('debug') && console && console.warn) {
        console.warn(
          'Select2: Using a hidden input with Select2 is no longer ' +
          'supported and may stop working in the future. It is recommended ' +
          'to use a `<select>` element instead.'
        );
      }
    }

    decorated.call(this, $element, options);
  }

  InputData.prototype.current = function (_, callback) {
    function getSelected (data, selectedIds) {
      var selected = [];

      if (data.selected || $.inArray(data.id, selectedIds) !== -1) {
        data.selected = true;
        selected.push(data);
      } else {
        data.selected = false;
      }

      if (data.children) {
        selected.push.apply(selected, getSelected(data.children, selectedIds));
      }

      return selected;
    }

    var selected = [];

    for (var d = 0; d < this._currentData.length; d++) {
      var data = this._currentData[d];

      selected.push.apply(
        selected,
        getSelected(
          data,
          this.$element.val().split(
            this._valueSeparator
          )
        )
      );
    }

    callback(selected);
  };

  InputData.prototype.select = function (_, data) {
    if (!this.options.get('multiple')) {
      this.current(function (allData) {
        $.map(allData, function (data) {
          data.selected = false;
        });
      });

      this.$element.val(data.id);
      this.$element.trigger('change');
    } else {
      var value = this.$element.val();
      value += this._valueSeparator + data.id;

      this.$element.val(value);
      this.$element.trigger('change');
    }
  };

  InputData.prototype.unselect = function (_, data) {
    var self = this;

    data.selected = false;

    this.current(function (allData) {
      var values = [];

      for (var d = 0; d < allData.length; d++) {
        var item = allData[d];

        if (data.id == item.id) {
          continue;
        }

        values.push(item.id);
      }

      self.$element.val(values.join(self._valueSeparator));
      self.$element.trigger('change');
    });
  };

  InputData.prototype.query = function (_, params, callback) {
    var results = [];

    for (var d = 0; d < this._currentData.length; d++) {
      var data = this._currentData[d];

      var matches = this.matches(params, data);

      if (matches !== null) {
        results.push(matches);
      }
    }

    callback({
      results: results
    });
  };

  InputData.prototype.addOptions = function (_, $options) {
    var options = $.map($options, function ($option) {
      return $.data($option[0], 'data');
    });

    this._currentData.push.apply(this._currentData, options);
  };

  return InputData;
});

S2.define('select2/compat/matcher',[
  'jquery'
], function ($) {
  function oldMatcher (matcher) {
    function wrappedMatcher (params, data) {
      var match = $.extend(true, {}, data);

      if (params.term == null || $.trim(params.term) === '') {
        return match;
      }

      if (data.children) {
        for (var c = data.children.length - 1; c >= 0; c--) {
          var child = data.children[c];

          // Check if the child object matches
          // The old matcher returned a boolean true or false
          var doesMatch = matcher(params.term, child.text, child);

          // If the child didn't match, pop it off
          if (!doesMatch) {
            match.children.splice(c, 1);
          }
        }

        if (match.children.length > 0) {
          return match;
        }
      }

      if (matcher(params.term, data.text, data)) {
        return match;
      }

      return null;
    }

    return wrappedMatcher;
  }

  return oldMatcher;
});

S2.define('select2/compat/query',[

], function () {
  function Query (decorated, $element, options) {
    if (options.get('debug') && window.console && console.warn) {
      console.warn(
        'Select2: The `query` option has been deprecated in favor of a ' +
        'custom data adapter that overrides the `query` method. Support ' +
        'will be removed for the `query` option in future versions of ' +
        'Select2.'
      );
    }

    decorated.call(this, $element, options);
  }

  Query.prototype.query = function (_, params, callback) {
    params.callback = callback;

    var query = this.options.get('query');

    query.call(null, params);
  };

  return Query;
});

S2.define('select2/dropdown/attachContainer',[

], function () {
  function AttachContainer (decorated, $element, options) {
    decorated.call(this, $element, options);
  }

  AttachContainer.prototype.position =
    function (decorated, $dropdown, $container) {
    var $dropdownContainer = $container.find('.dropdown-wrapper');
    $dropdownContainer.append($dropdown);

    $dropdown.addClass('select2-dropdown--below');
    $container.addClass('select2-container--below');
  };

  return AttachContainer;
});

S2.define('select2/dropdown/stopPropagation',[

], function () {
  function StopPropagation () { }

  StopPropagation.prototype.bind = function (decorated, container, $container) {
    decorated.call(this, container, $container);

    var stoppedEvents = [
    'blur',
    'change',
    'click',
    'dblclick',
    'focus',
    'focusin',
    'focusout',
    'input',
    'keydown',
    'keyup',
    'keypress',
    'mousedown',
    'mouseenter',
    'mouseleave',
    'mousemove',
    'mouseover',
    'mouseup',
    'search',
    'touchend',
    'touchstart'
    ];

    this.$dropdown.on(stoppedEvents.join(' '), function (evt) {
      evt.stopPropagation();
    });
  };

  return StopPropagation;
});

S2.define('select2/selection/stopPropagation',[

], function () {
  function StopPropagation () { }

  StopPropagation.prototype.bind = function (decorated, container, $container) {
    decorated.call(this, container, $container);

    var stoppedEvents = [
      'blur',
      'change',
      'click',
      'dblclick',
      'focus',
      'focusin',
      'focusout',
      'input',
      'keydown',
      'keyup',
      'keypress',
      'mousedown',
      'mouseenter',
      'mouseleave',
      'mousemove',
      'mouseover',
      'mouseup',
      'search',
      'touchend',
      'touchstart'
    ];

    this.$selection.on(stoppedEvents.join(' '), function (evt) {
      evt.stopPropagation();
    });
  };

  return StopPropagation;
});

S2.define('jquery.select2',[
  'jquery',
  'require',

  './select2/core',
  './select2/defaults'
], function ($, require, Select2, Defaults) {
  // Force jQuery.mousewheel to be loaded if it hasn't already
  require('jquery.mousewheel');

  if ($.fn.select2 == null) {
    // All methods that should return the element
    var thisMethods = ['open', 'close', 'destroy'];

    $.fn.select2 = function (options) {
      options = options || {};

      if (typeof options === 'object') {
        this.each(function () {
          var instanceOptions = $.extend({}, options, true);

          var instance = new Select2($(this), instanceOptions);
        });

        return this;
      } else if (typeof options === 'string') {
        var instance = this.data('select2');

        if (instance == null && window.console && console.error) {
          console.error(
            'The select2(\'' + options + '\') method was called on an ' +
            'element that is not using Select2.'
          );
        }

        var args = Array.prototype.slice.call(arguments, 1);

        var ret = instance[options](args);

        // Check if we should be returning `this`
        if ($.inArray(options, thisMethods) > -1) {
          return this;
        }

        return ret;
      } else {
        throw new Error('Invalid arguments for Select2: ' + options);
      }
    };
  }

  if ($.fn.select2.defaults == null) {
    $.fn.select2.defaults = Defaults;
  }

  return Select2;
});

/*!
 * jQuery Mousewheel 3.1.12
 *
 * Copyright 2014 jQuery Foundation and other contributors
 * Released under the MIT license.
 * http://jquery.org/license
 */

(function (factory) {
    if ( typeof S2.define === 'function' && S2.define.amd ) {
        // AMD. Register as an anonymous module.
        S2.define('jquery.mousewheel',['jquery'], factory);
    } else if (typeof exports === 'object') {
        // Node/CommonJS style for Browserify
        module.exports = factory;
    } else {
        // Browser globals
        factory(jQuery);
    }
}(function ($) {

    var toFix  = ['wheel', 'mousewheel', 'DOMMouseScroll', 'MozMousePixelScroll'],
        toBind = ( 'onwheel' in document || document.documentMode >= 9 ) ?
                    ['wheel'] : ['mousewheel', 'DomMouseScroll', 'MozMousePixelScroll'],
        slice  = Array.prototype.slice,
        nullLowestDeltaTimeout, lowestDelta;

    if ( $.event.fixHooks ) {
        for ( var i = toFix.length; i; ) {
            $.event.fixHooks[ toFix[--i] ] = $.event.mouseHooks;
        }
    }

    var special = $.event.special.mousewheel = {
        version: '3.1.12',

        setup: function() {
            if ( this.addEventListener ) {
                for ( var i = toBind.length; i; ) {
                    this.addEventListener( toBind[--i], handler, false );
                }
            } else {
                this.onmousewheel = handler;
            }
            // Store the line height and page height for this particular element
            $.data(this, 'mousewheel-line-height', special.getLineHeight(this));
            $.data(this, 'mousewheel-page-height', special.getPageHeight(this));
        },

        teardown: function() {
            if ( this.removeEventListener ) {
                for ( var i = toBind.length; i; ) {
                    this.removeEventListener( toBind[--i], handler, false );
                }
            } else {
                this.onmousewheel = null;
            }
            // Clean up the data we added to the element
            $.removeData(this, 'mousewheel-line-height');
            $.removeData(this, 'mousewheel-page-height');
        },

        getLineHeight: function(elem) {
            var $elem = $(elem),
                $parent = $elem['offsetParent' in $.fn ? 'offsetParent' : 'parent']();
            if (!$parent.length) {
                $parent = $('body');
            }
            return parseInt($parent.css('fontSize'), 10) || parseInt($elem.css('fontSize'), 10) || 16;
        },

        getPageHeight: function(elem) {
            return $(elem).height();
        },

        settings: {
            adjustOldDeltas: true, // see shouldAdjustOldDeltas() below
            normalizeOffset: true  // calls getBoundingClientRect for each event
        }
    };

    $.fn.extend({
        mousewheel: function(fn) {
            return fn ? this.bind('mousewheel', fn) : this.trigger('mousewheel');
        },

        unmousewheel: function(fn) {
            return this.unbind('mousewheel', fn);
        }
    });


    function handler(event) {
        var orgEvent   = event || window.event,
            args       = slice.call(arguments, 1),
            delta      = 0,
            deltaX     = 0,
            deltaY     = 0,
            absDelta   = 0,
            offsetX    = 0,
            offsetY    = 0;
        event = $.event.fix(orgEvent);
        event.type = 'mousewheel';

        // Old school scrollwheel delta
        if ( 'detail'      in orgEvent ) { deltaY = orgEvent.detail * -1;      }
        if ( 'wheelDelta'  in orgEvent ) { deltaY = orgEvent.wheelDelta;       }
        if ( 'wheelDeltaY' in orgEvent ) { deltaY = orgEvent.wheelDeltaY;      }
        if ( 'wheelDeltaX' in orgEvent ) { deltaX = orgEvent.wheelDeltaX * -1; }

        // Firefox < 17 horizontal scrolling related to DOMMouseScroll event
        if ( 'axis' in orgEvent && orgEvent.axis === orgEvent.HORIZONTAL_AXIS ) {
            deltaX = deltaY * -1;
            deltaY = 0;
        }

        // Set delta to be deltaY or deltaX if deltaY is 0 for backwards compatabilitiy
        delta = deltaY === 0 ? deltaX : deltaY;

        // New school wheel delta (wheel event)
        if ( 'deltaY' in orgEvent ) {
            deltaY = orgEvent.deltaY * -1;
            delta  = deltaY;
        }
        if ( 'deltaX' in orgEvent ) {
            deltaX = orgEvent.deltaX;
            if ( deltaY === 0 ) { delta  = deltaX * -1; }
        }

        // No change actually happened, no reason to go any further
        if ( deltaY === 0 && deltaX === 0 ) { return; }

        // Need to convert lines and pages to pixels if we aren't already in pixels
        // There are three delta modes:
        //   * deltaMode 0 is by pixels, nothing to do
        //   * deltaMode 1 is by lines
        //   * deltaMode 2 is by pages
        if ( orgEvent.deltaMode === 1 ) {
            var lineHeight = $.data(this, 'mousewheel-line-height');
            delta  *= lineHeight;
            deltaY *= lineHeight;
            deltaX *= lineHeight;
        } else if ( orgEvent.deltaMode === 2 ) {
            var pageHeight = $.data(this, 'mousewheel-page-height');
            delta  *= pageHeight;
            deltaY *= pageHeight;
            deltaX *= pageHeight;
        }

        // Store lowest absolute delta to normalize the delta values
        absDelta = Math.max( Math.abs(deltaY), Math.abs(deltaX) );

        if ( !lowestDelta || absDelta < lowestDelta ) {
            lowestDelta = absDelta;

            // Adjust older deltas if necessary
            if ( shouldAdjustOldDeltas(orgEvent, absDelta) ) {
                lowestDelta /= 40;
            }
        }

        // Adjust older deltas if necessary
        if ( shouldAdjustOldDeltas(orgEvent, absDelta) ) {
            // Divide all the things by 40!
            delta  /= 40;
            deltaX /= 40;
            deltaY /= 40;
        }

        // Get a whole, normalized value for the deltas
        delta  = Math[ delta  >= 1 ? 'floor' : 'ceil' ](delta  / lowestDelta);
        deltaX = Math[ deltaX >= 1 ? 'floor' : 'ceil' ](deltaX / lowestDelta);
        deltaY = Math[ deltaY >= 1 ? 'floor' : 'ceil' ](deltaY / lowestDelta);

        // Normalise offsetX and offsetY properties
        if ( special.settings.normalizeOffset && this.getBoundingClientRect ) {
            var boundingRect = this.getBoundingClientRect();
            offsetX = event.clientX - boundingRect.left;
            offsetY = event.clientY - boundingRect.top;
        }

        // Add information to the event object
        event.deltaX = deltaX;
        event.deltaY = deltaY;
        event.deltaFactor = lowestDelta;
        event.offsetX = offsetX;
        event.offsetY = offsetY;
        // Go ahead and set deltaMode to 0 since we converted to pixels
        // Although this is a little odd since we overwrite the deltaX/Y
        // properties with normalized deltas.
        event.deltaMode = 0;

        // Add event and delta to the front of the arguments
        args.unshift(event, delta, deltaX, deltaY);

        // Clearout lowestDelta after sometime to better
        // handle multiple device types that give different
        // a different lowestDelta
        // Ex: trackpad = 3 and mouse wheel = 120
        if (nullLowestDeltaTimeout) { clearTimeout(nullLowestDeltaTimeout); }
        nullLowestDeltaTimeout = setTimeout(nullLowestDelta, 200);

        return ($.event.dispatch || $.event.handle).apply(this, args);
    }

    function nullLowestDelta() {
        lowestDelta = null;
    }

    function shouldAdjustOldDeltas(orgEvent, absDelta) {
        // If this is an older event and the delta is divisable by 120,
        // then we are assuming that the browser is treating this as an
        // older mouse wheel event and that we should divide the deltas
        // by 40 to try and get a more usable deltaFactor.
        // Side note, this actually impacts the reported scroll distance
        // in older browsers and can cause scrolling to be slower than native.
        // Turn this off by setting $.event.special.mousewheel.settings.adjustOldDeltas to false.
        return special.settings.adjustOldDeltas && orgEvent.type === 'mousewheel' && absDelta % 120 === 0;
    }

}));

  // Return the AMD loader configuration so it can be used outside of this file
  return {
    define: S2.define,
    require: S2.require
  };
}());

  // Autoload the jQuery bindings
  // We know that all of the modules exist above this, so we're safe
  var select2 = S2.require('jquery.select2');

  // Hold the AMD module references on the jQuery function that was just loaded
  // This allows Select2 to use the internal loader outside of this file, such
  // as in the language files.
  jQuery.fn.select2.amd = S2;

  // Return the Select2 instance for anyone who is importing it.
  return select2;
}));

/**
 * Version: 1.0 Alpha-1 
 * Build Date: 13-Nov-2007
 * Copyright (c) 2006-2007, Coolite Inc. (http://www.coolite.com/). All rights reserved.
 * License: Licensed under The MIT License. See license.txt and http://www.datejs.com/license/. 
 * Website: http://www.datejs.com/ or http://www.coolite.com/datejs/
 */
Date.CultureInfo={name:"en-US",englishName:"English (United States)",nativeName:"English (United States)",dayNames:["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"],abbreviatedDayNames:["Sun","Mon","Tue","Wed","Thu","Fri","Sat"],shortestDayNames:["Su","Mo","Tu","We","Th","Fr","Sa"],firstLetterDayNames:["S","M","T","W","T","F","S"],monthNames:["January","February","March","April","May","June","July","August","September","October","November","December"],abbreviatedMonthNames:["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec"],amDesignator:"AM",pmDesignator:"PM",firstDayOfWeek:0,twoDigitYearMax:2029,dateElementOrder:"mdy",formatPatterns:{shortDate:"M/d/yyyy",longDate:"dddd, MMMM dd, yyyy",shortTime:"h:mm tt",longTime:"h:mm:ss tt",fullDateTime:"dddd, MMMM dd, yyyy h:mm:ss tt",sortableDateTime:"yyyy-MM-ddTHH:mm:ss",universalSortableDateTime:"yyyy-MM-dd HH:mm:ssZ",rfc1123:"ddd, dd MMM yyyy HH:mm:ss GMT",monthDay:"MMMM dd",yearMonth:"MMMM, yyyy"},regexPatterns:{jan:/^jan(uary)?/i,feb:/^feb(ruary)?/i,mar:/^mar(ch)?/i,apr:/^apr(il)?/i,may:/^may/i,jun:/^jun(e)?/i,jul:/^jul(y)?/i,aug:/^aug(ust)?/i,sep:/^sep(t(ember)?)?/i,oct:/^oct(ober)?/i,nov:/^nov(ember)?/i,dec:/^dec(ember)?/i,sun:/^su(n(day)?)?/i,mon:/^mo(n(day)?)?/i,tue:/^tu(e(s(day)?)?)?/i,wed:/^we(d(nesday)?)?/i,thu:/^th(u(r(s(day)?)?)?)?/i,fri:/^fr(i(day)?)?/i,sat:/^sa(t(urday)?)?/i,future:/^next/i,past:/^last|past|prev(ious)?/i,add:/^(\+|after|from)/i,subtract:/^(\-|before|ago)/i,yesterday:/^yesterday/i,today:/^t(oday)?/i,tomorrow:/^tomorrow/i,now:/^n(ow)?/i,millisecond:/^ms|milli(second)?s?/i,second:/^sec(ond)?s?/i,minute:/^min(ute)?s?/i,hour:/^h(ou)?rs?/i,week:/^w(ee)?k/i,month:/^m(o(nth)?s?)?/i,day:/^d(ays?)?/i,year:/^y((ea)?rs?)?/i,shortMeridian:/^(a|p)/i,longMeridian:/^(a\.?m?\.?|p\.?m?\.?)/i,timezone:/^((e(s|d)t|c(s|d)t|m(s|d)t|p(s|d)t)|((gmt)?\s*(\+|\-)\s*\d\d\d\d?)|gmt)/i,ordinalSuffix:/^\s*(st|nd|rd|th)/i,timeContext:/^\s*(\:|a|p)/i},abbreviatedTimeZoneStandard:{GMT:"-000",EST:"-0400",CST:"-0500",MST:"-0600",PST:"-0700"},abbreviatedTimeZoneDST:{GMT:"-000",EDT:"-0500",CDT:"-0600",MDT:"-0700",PDT:"-0800"}};
Date.getMonthNumberFromName=function(name){var n=Date.CultureInfo.monthNames,m=Date.CultureInfo.abbreviatedMonthNames,s=name.toLowerCase();for(var i=0;i<n.length;i++){if(n[i].toLowerCase()==s||m[i].toLowerCase()==s){return i;}}
return-1;};Date.getDayNumberFromName=function(name){var n=Date.CultureInfo.dayNames,m=Date.CultureInfo.abbreviatedDayNames,o=Date.CultureInfo.shortestDayNames,s=name.toLowerCase();for(var i=0;i<n.length;i++){if(n[i].toLowerCase()==s||m[i].toLowerCase()==s){return i;}}
return-1;};Date.isLeapYear=function(year){return(((year%4===0)&&(year%100!==0))||(year%400===0));};Date.getDaysInMonth=function(year,month){return[31,(Date.isLeapYear(year)?29:28),31,30,31,30,31,31,30,31,30,31][month];};Date.getTimezoneOffset=function(s,dst){return(dst||false)?Date.CultureInfo.abbreviatedTimeZoneDST[s.toUpperCase()]:Date.CultureInfo.abbreviatedTimeZoneStandard[s.toUpperCase()];};Date.getTimezoneAbbreviation=function(offset,dst){var n=(dst||false)?Date.CultureInfo.abbreviatedTimeZoneDST:Date.CultureInfo.abbreviatedTimeZoneStandard,p;for(p in n){if(n[p]===offset){return p;}}
return null;};Date.prototype.clone=function(){return new Date(this.getTime());};Date.prototype.compareTo=function(date){if(isNaN(this)){throw new Error(this);}
if(date instanceof Date&&!isNaN(date)){return(this>date)?1:(this<date)?-1:0;}else{throw new TypeError(date);}};Date.prototype.equals=function(date){return(this.compareTo(date)===0);};Date.prototype.between=function(start,end){var t=this.getTime();return t>=start.getTime()&&t<=end.getTime();};Date.prototype.addMilliseconds=function(value){this.setMilliseconds(this.getMilliseconds()+value);return this;};Date.prototype.addSeconds=function(value){return this.addMilliseconds(value*1000);};Date.prototype.addMinutes=function(value){return this.addMilliseconds(value*60000);};Date.prototype.addHours=function(value){return this.addMilliseconds(value*3600000);};Date.prototype.addDays=function(value){return this.addMilliseconds(value*86400000);};Date.prototype.addWeeks=function(value){return this.addMilliseconds(value*604800000);};Date.prototype.addMonths=function(value){var n=this.getDate();this.setDate(1);this.setMonth(this.getMonth()+value);this.setDate(Math.min(n,this.getDaysInMonth()));return this;};Date.prototype.addYears=function(value){return this.addMonths(value*12);};Date.prototype.add=function(config){if(typeof config=="number"){this._orient=config;return this;}
var x=config;if(x.millisecond||x.milliseconds){this.addMilliseconds(x.millisecond||x.milliseconds);}
if(x.second||x.seconds){this.addSeconds(x.second||x.seconds);}
if(x.minute||x.minutes){this.addMinutes(x.minute||x.minutes);}
if(x.hour||x.hours){this.addHours(x.hour||x.hours);}
if(x.month||x.months){this.addMonths(x.month||x.months);}
if(x.year||x.years){this.addYears(x.year||x.years);}
if(x.day||x.days){this.addDays(x.day||x.days);}
return this;};Date._validate=function(value,min,max,name){if(typeof value!="number"){throw new TypeError(value+" is not a Number.");}else if(value<min||value>max){throw new RangeError(value+" is not a valid value for "+name+".");}
return true;};Date.validateMillisecond=function(n){return Date._validate(n,0,999,"milliseconds");};Date.validateSecond=function(n){return Date._validate(n,0,59,"seconds");};Date.validateMinute=function(n){return Date._validate(n,0,59,"minutes");};Date.validateHour=function(n){return Date._validate(n,0,23,"hours");};Date.validateDay=function(n,year,month){return Date._validate(n,1,Date.getDaysInMonth(year,month),"days");};Date.validateMonth=function(n){return Date._validate(n,0,11,"months");};Date.validateYear=function(n){return Date._validate(n,1,9999,"seconds");};Date.prototype.set=function(config){var x=config;if(!x.millisecond&&x.millisecond!==0){x.millisecond=-1;}
if(!x.second&&x.second!==0){x.second=-1;}
if(!x.minute&&x.minute!==0){x.minute=-1;}
if(!x.hour&&x.hour!==0){x.hour=-1;}
if(!x.day&&x.day!==0){x.day=-1;}
if(!x.month&&x.month!==0){x.month=-1;}
if(!x.year&&x.year!==0){x.year=-1;}
if(x.millisecond!=-1&&Date.validateMillisecond(x.millisecond)){this.addMilliseconds(x.millisecond-this.getMilliseconds());}
if(x.second!=-1&&Date.validateSecond(x.second)){this.addSeconds(x.second-this.getSeconds());}
if(x.minute!=-1&&Date.validateMinute(x.minute)){this.addMinutes(x.minute-this.getMinutes());}
if(x.hour!=-1&&Date.validateHour(x.hour)){this.addHours(x.hour-this.getHours());}
if(x.month!==-1&&Date.validateMonth(x.month)){this.addMonths(x.month-this.getMonth());}
if(x.year!=-1&&Date.validateYear(x.year)){this.addYears(x.year-this.getFullYear());}
if(x.day!=-1&&Date.validateDay(x.day,this.getFullYear(),this.getMonth())){this.addDays(x.day-this.getDate());}
if(x.timezone){this.setTimezone(x.timezone);}
if(x.timezoneOffset){this.setTimezoneOffset(x.timezoneOffset);}
return this;};Date.prototype.clearTime=function(){this.setHours(0);this.setMinutes(0);this.setSeconds(0);this.setMilliseconds(0);return this;};Date.prototype.isLeapYear=function(){var y=this.getFullYear();return(((y%4===0)&&(y%100!==0))||(y%400===0));};Date.prototype.isWeekday=function(){return!(this.is().sat()||this.is().sun());};Date.prototype.getDaysInMonth=function(){return Date.getDaysInMonth(this.getFullYear(),this.getMonth());};Date.prototype.moveToFirstDayOfMonth=function(){return this.set({day:1});};Date.prototype.moveToLastDayOfMonth=function(){return this.set({day:this.getDaysInMonth()});};Date.prototype.moveToDayOfWeek=function(day,orient){var diff=(day-this.getDay()+7*(orient||+1))%7;return this.addDays((diff===0)?diff+=7*(orient||+1):diff);};Date.prototype.moveToMonth=function(month,orient){var diff=(month-this.getMonth()+12*(orient||+1))%12;return this.addMonths((diff===0)?diff+=12*(orient||+1):diff);};Date.prototype.getDayOfYear=function(){return Math.floor((this-new Date(this.getFullYear(),0,1))/86400000);};Date.prototype.getWeekOfYear=function(firstDayOfWeek){var y=this.getFullYear(),m=this.getMonth(),d=this.getDate();var dow=firstDayOfWeek||Date.CultureInfo.firstDayOfWeek;var offset=7+1-new Date(y,0,1).getDay();if(offset==8){offset=1;}
var daynum=((Date.UTC(y,m,d,0,0,0)-Date.UTC(y,0,1,0,0,0))/86400000)+1;var w=Math.floor((daynum-offset+7)/7);if(w===dow){y--;var prevOffset=7+1-new Date(y,0,1).getDay();if(prevOffset==2||prevOffset==8){w=53;}else{w=52;}}
return w;};Date.prototype.isDST=function(){console.log('isDST');return this.toString().match(/(E|C|M|P)(S|D)T/)[2]=="D";};Date.prototype.getTimezone=function(){return Date.getTimezoneAbbreviation(this.getUTCOffset,this.isDST());};Date.prototype.setTimezoneOffset=function(s){var here=this.getTimezoneOffset(),there=Number(s)*-6/10;this.addMinutes(there-here);return this;};Date.prototype.setTimezone=function(s){return this.setTimezoneOffset(Date.getTimezoneOffset(s));};Date.prototype.getUTCOffset=function(){var n=this.getTimezoneOffset()*-10/6,r;if(n<0){r=(n-10000).toString();return r[0]+r.substr(2);}else{r=(n+10000).toString();return"+"+r.substr(1);}};Date.prototype.getDayName=function(abbrev){return abbrev?Date.CultureInfo.abbreviatedDayNames[this.getDay()]:Date.CultureInfo.dayNames[this.getDay()];};Date.prototype.getMonthName=function(abbrev){return abbrev?Date.CultureInfo.abbreviatedMonthNames[this.getMonth()]:Date.CultureInfo.monthNames[this.getMonth()];};Date.prototype._toString=Date.prototype.toString;Date.prototype.toString=function(format){var self=this;var p=function p(s){return(s.toString().length==1)?"0"+s:s;};return format?format.replace(/dd?d?d?|MM?M?M?|yy?y?y?|hh?|HH?|mm?|ss?|tt?|zz?z?/g,function(format){switch(format){case"hh":return p(self.getHours()<13?self.getHours():(self.getHours()-12));case"h":return self.getHours()<13?self.getHours():(self.getHours()-12);case"HH":return p(self.getHours());case"H":return self.getHours();case"mm":return p(self.getMinutes());case"m":return self.getMinutes();case"ss":return p(self.getSeconds());case"s":return self.getSeconds();case"yyyy":return self.getFullYear();case"yy":return self.getFullYear().toString().substring(2,4);case"dddd":return self.getDayName();case"ddd":return self.getDayName(true);case"dd":return p(self.getDate());case"d":return self.getDate().toString();case"MMMM":return self.getMonthName();case"MMM":return self.getMonthName(true);case"MM":return p((self.getMonth()+1));case"M":return self.getMonth()+1;case"t":return self.getHours()<12?Date.CultureInfo.amDesignator.substring(0,1):Date.CultureInfo.pmDesignator.substring(0,1);case"tt":return self.getHours()<12?Date.CultureInfo.amDesignator:Date.CultureInfo.pmDesignator;case"zzz":case"zz":case"z":return"";}}):this._toString();};
Date.now=function(){return new Date();};Date.today=function(){return Date.now().clearTime();};Date.prototype._orient=+1;Date.prototype.next=function(){this._orient=+1;return this;};Date.prototype.last=Date.prototype.prev=Date.prototype.previous=function(){this._orient=-1;return this;};Date.prototype._is=false;Date.prototype.is=function(){this._is=true;return this;};Number.prototype._dateElement="day";Number.prototype.fromNow=function(){var c={};c[this._dateElement]=this;return Date.now().add(c);};Number.prototype.ago=function(){var c={};c[this._dateElement]=this*-1;return Date.now().add(c);};(function(){var $D=Date.prototype,$N=Number.prototype;var dx=("sunday monday tuesday wednesday thursday friday saturday").split(/\s/),mx=("january february march april may june july august september october november december").split(/\s/),px=("Millisecond Second Minute Hour Day Week Month Year").split(/\s/),de;var df=function(n){return function(){if(this._is){this._is=false;return this.getDay()==n;}
return this.moveToDayOfWeek(n,this._orient);};};for(var i=0;i<dx.length;i++){$D[dx[i]]=$D[dx[i].substring(0,3)]=df(i);}
var mf=function(n){return function(){if(this._is){this._is=false;return this.getMonth()===n;}
return this.moveToMonth(n,this._orient);};};for(var j=0;j<mx.length;j++){$D[mx[j]]=$D[mx[j].substring(0,3)]=mf(j);}
var ef=function(j){return function(){if(j.substring(j.length-1)!="s"){j+="s";}
return this["add"+j](this._orient);};};var nf=function(n){return function(){this._dateElement=n;return this;};};for(var k=0;k<px.length;k++){de=px[k].toLowerCase();$D[de]=$D[de+"s"]=ef(px[k]);$N[de]=$N[de+"s"]=nf(de);}}());Date.prototype.toJSONString=function(){return this.toString("yyyy-MM-ddThh:mm:ssZ");};Date.prototype.toShortDateString=function(){return this.toString(Date.CultureInfo.formatPatterns.shortDatePattern);};Date.prototype.toLongDateString=function(){return this.toString(Date.CultureInfo.formatPatterns.longDatePattern);};Date.prototype.toShortTimeString=function(){return this.toString(Date.CultureInfo.formatPatterns.shortTimePattern);};Date.prototype.toLongTimeString=function(){return this.toString(Date.CultureInfo.formatPatterns.longTimePattern);};Date.prototype.getOrdinal=function(){switch(this.getDate()){case 1:case 21:case 31:return"st";case 2:case 22:return"nd";case 3:case 23:return"rd";default:return"th";}};
(function(){Date.Parsing={Exception:function(s){this.message="Parse error at '"+s.substring(0,10)+" ...'";}};var $P=Date.Parsing;var _=$P.Operators={rtoken:function(r){return function(s){var mx=s.match(r);if(mx){return([mx[0],s.substring(mx[0].length)]);}else{throw new $P.Exception(s);}};},token:function(s){return function(s){return _.rtoken(new RegExp("^\s*"+s+"\s*"))(s);};},stoken:function(s){return _.rtoken(new RegExp("^"+s));},until:function(p){return function(s){var qx=[],rx=null;while(s.length){try{rx=p.call(this,s);}catch(e){qx.push(rx[0]);s=rx[1];continue;}
break;}
return[qx,s];};},many:function(p){return function(s){var rx=[],r=null;while(s.length){try{r=p.call(this,s);}catch(e){return[rx,s];}
rx.push(r[0]);s=r[1];}
return[rx,s];};},optional:function(p){return function(s){var r=null;try{r=p.call(this,s);}catch(e){return[null,s];}
return[r[0],r[1]];};},not:function(p){return function(s){try{p.call(this,s);}catch(e){return[null,s];}
throw new $P.Exception(s);};},ignore:function(p){return p?function(s){var r=null;r=p.call(this,s);return[null,r[1]];}:null;},product:function(){var px=arguments[0],qx=Array.prototype.slice.call(arguments,1),rx=[];for(var i=0;i<px.length;i++){rx.push(_.each(px[i],qx));}
return rx;},cache:function(rule){var cache={},r=null;return function(s){try{r=cache[s]=(cache[s]||rule.call(this,s));}catch(e){r=cache[s]=e;}
if(r instanceof $P.Exception){throw r;}else{return r;}};},any:function(){var px=arguments;return function(s){var r=null;for(var i=0;i<px.length;i++){if(px[i]==null){continue;}
try{r=(px[i].call(this,s));}catch(e){r=null;}
if(r){return r;}}
throw new $P.Exception(s);};},each:function(){var px=arguments;return function(s){var rx=[],r=null;for(var i=0;i<px.length;i++){if(px[i]==null){continue;}
try{r=(px[i].call(this,s));}catch(e){throw new $P.Exception(s);}
rx.push(r[0]);s=r[1];}
return[rx,s];};},all:function(){var px=arguments,_=_;return _.each(_.optional(px));},sequence:function(px,d,c){d=d||_.rtoken(/^\s*/);c=c||null;if(px.length==1){return px[0];}
return function(s){var r=null,q=null;var rx=[];for(var i=0;i<px.length;i++){try{r=px[i].call(this,s);}catch(e){break;}
rx.push(r[0]);try{q=d.call(this,r[1]);}catch(ex){q=null;break;}
s=q[1];}
if(!r){throw new $P.Exception(s);}
if(q){throw new $P.Exception(q[1]);}
if(c){try{r=c.call(this,r[1]);}catch(ey){throw new $P.Exception(r[1]);}}
return[rx,(r?r[1]:s)];};},between:function(d1,p,d2){d2=d2||d1;var _fn=_.each(_.ignore(d1),p,_.ignore(d2));return function(s){var rx=_fn.call(this,s);return[[rx[0][0],r[0][2]],rx[1]];};},list:function(p,d,c){d=d||_.rtoken(/^\s*/);c=c||null;return(p instanceof Array?_.each(_.product(p.slice(0,-1),_.ignore(d)),p.slice(-1),_.ignore(c)):_.each(_.many(_.each(p,_.ignore(d))),px,_.ignore(c)));},set:function(px,d,c){d=d||_.rtoken(/^\s*/);c=c||null;return function(s){var r=null,p=null,q=null,rx=null,best=[[],s],last=false;for(var i=0;i<px.length;i++){q=null;p=null;r=null;last=(px.length==1);try{r=px[i].call(this,s);}catch(e){continue;}
rx=[[r[0]],r[1]];if(r[1].length>0&&!last){try{q=d.call(this,r[1]);}catch(ex){last=true;}}else{last=true;}
if(!last&&q[1].length===0){last=true;}
if(!last){var qx=[];for(var j=0;j<px.length;j++){if(i!=j){qx.push(px[j]);}}
p=_.set(qx,d).call(this,q[1]);if(p[0].length>0){rx[0]=rx[0].concat(p[0]);rx[1]=p[1];}}
if(rx[1].length<best[1].length){best=rx;}
if(best[1].length===0){break;}}
if(best[0].length===0){return best;}
if(c){try{q=c.call(this,best[1]);}catch(ey){throw new $P.Exception(best[1]);}
best[1]=q[1];}
return best;};},forward:function(gr,fname){return function(s){return gr[fname].call(this,s);};},replace:function(rule,repl){return function(s){var r=rule.call(this,s);return[repl,r[1]];};},process:function(rule,fn){return function(s){var r=rule.call(this,s);return[fn.call(this,r[0]),r[1]];};},min:function(min,rule){return function(s){var rx=rule.call(this,s);if(rx[0].length<min){throw new $P.Exception(s);}
return rx;};}};var _generator=function(op){return function(){var args=null,rx=[];if(arguments.length>1){args=Array.prototype.slice.call(arguments);}else if(arguments[0]instanceof Array){args=arguments[0];}
if(args){for(var i=0,px=args.shift();i<px.length;i++){args.unshift(px[i]);rx.push(op.apply(null,args));args.shift();return rx;}}else{return op.apply(null,arguments);}};};var gx="optional not ignore cache".split(/\s/);for(var i=0;i<gx.length;i++){_[gx[i]]=_generator(_[gx[i]]);}
var _vector=function(op){return function(){if(arguments[0]instanceof Array){return op.apply(null,arguments[0]);}else{return op.apply(null,arguments);}};};var vx="each any all".split(/\s/);for(var j=0;j<vx.length;j++){_[vx[j]]=_vector(_[vx[j]]);}}());(function(){var flattenAndCompact=function(ax){var rx=[];for(var i=0;i<ax.length;i++){if(ax[i]instanceof Array){rx=rx.concat(flattenAndCompact(ax[i]));}else{if(ax[i]){rx.push(ax[i]);}}}
return rx;};Date.Grammar={};Date.Translator={hour:function(s){return function(){this.hour=Number(s);};},minute:function(s){return function(){this.minute=Number(s);};},second:function(s){return function(){this.second=Number(s);};},meridian:function(s){return function(){this.meridian=s.slice(0,1).toLowerCase();};},timezone:function(s){return function(){var n=s.replace(/[^\d\+\-]/g,"");if(n.length){this.timezoneOffset=Number(n);}else{this.timezone=s.toLowerCase();}};},day:function(x){var s=x[0];return function(){this.day=Number(s.match(/\d+/)[0]);};},month:function(s){return function(){this.month=((s.length==3)?Date.getMonthNumberFromName(s):(Number(s)-1));};},year:function(s){return function(){var n=Number(s);this.year=((s.length>2)?n:(n+(((n+2000)<Date.CultureInfo.twoDigitYearMax)?2000:1900)));};},rday:function(s){return function(){switch(s){case"yesterday":this.days=-1;break;case"tomorrow":this.days=1;break;case"today":this.days=0;break;case"now":this.days=0;this.now=true;break;}};},finishExact:function(x){x=(x instanceof Array)?x:[x];var now=new Date();this.year=now.getFullYear();this.month=now.getMonth();this.day=1;this.hour=0;this.minute=0;this.second=0;for(var i=0;i<x.length;i++){if(x[i]){x[i].call(this);}}
this.hour=(this.meridian=="p"&&this.hour<13)?this.hour+12:this.hour;if(this.day>Date.getDaysInMonth(this.year,this.month)){throw new RangeError(this.day+" is not a valid value for days.");}
var r=new Date(this.year,this.month,this.day,this.hour,this.minute,this.second);if(this.timezone){r.set({timezone:this.timezone});}else if(this.timezoneOffset){r.set({timezoneOffset:this.timezoneOffset});}
return r;},finish:function(x){x=(x instanceof Array)?flattenAndCompact(x):[x];if(x.length===0){return null;}
for(var i=0;i<x.length;i++){if(typeof x[i]=="function"){x[i].call(this);}}
if(this.now){return new Date();}
var today=Date.today();var method=null;var expression=!!(this.days!=null||this.orient||this.operator);if(expression){var gap,mod,orient;orient=((this.orient=="past"||this.operator=="subtract")?-1:1);if(this.weekday){this.unit="day";gap=(Date.getDayNumberFromName(this.weekday)-today.getDay());mod=7;this.days=gap?((gap+(orient*mod))%mod):(orient*mod);}
if(this.month){this.unit="month";gap=(this.month-today.getMonth());mod=12;this.months=gap?((gap+(orient*mod))%mod):(orient*mod);this.month=null;}
if(!this.unit){this.unit="day";}
if(this[this.unit+"s"]==null||this.operator!=null){if(!this.value){this.value=1;}
if(this.unit=="week"){this.unit="day";this.value=this.value*7;}
this[this.unit+"s"]=this.value*orient;}
return today.add(this);}else{if(this.meridian&&this.hour){this.hour=(this.hour<13&&this.meridian=="p")?this.hour+12:this.hour;}
if(this.weekday&&!this.day){this.day=(today.addDays((Date.getDayNumberFromName(this.weekday)-today.getDay()))).getDate();}
if(this.month&&!this.day){this.day=1;}
return today.set(this);}}};var _=Date.Parsing.Operators,g=Date.Grammar,t=Date.Translator,_fn;g.datePartDelimiter=_.rtoken(/^([\s\-\.\,\/\x27]+)/);g.timePartDelimiter=_.stoken(":");g.whiteSpace=_.rtoken(/^\s*/);g.generalDelimiter=_.rtoken(/^(([\s\,]|at|on)+)/);var _C={};g.ctoken=function(keys){var fn=_C[keys];if(!fn){var c=Date.CultureInfo.regexPatterns;var kx=keys.split(/\s+/),px=[];for(var i=0;i<kx.length;i++){px.push(_.replace(_.rtoken(c[kx[i]]),kx[i]));}
fn=_C[keys]=_.any.apply(null,px);}
return fn;};g.ctoken2=function(key){return _.rtoken(Date.CultureInfo.regexPatterns[key]);};g.h=_.cache(_.process(_.rtoken(/^(0[0-9]|1[0-2]|[1-9])/),t.hour));g.hh=_.cache(_.process(_.rtoken(/^(0[0-9]|1[0-2])/),t.hour));g.H=_.cache(_.process(_.rtoken(/^([0-1][0-9]|2[0-3]|[0-9])/),t.hour));g.HH=_.cache(_.process(_.rtoken(/^([0-1][0-9]|2[0-3])/),t.hour));g.m=_.cache(_.process(_.rtoken(/^([0-5][0-9]|[0-9])/),t.minute));g.mm=_.cache(_.process(_.rtoken(/^[0-5][0-9]/),t.minute));g.s=_.cache(_.process(_.rtoken(/^([0-5][0-9]|[0-9])/),t.second));g.ss=_.cache(_.process(_.rtoken(/^[0-5][0-9]/),t.second));g.hms=_.cache(_.sequence([g.H,g.mm,g.ss],g.timePartDelimiter));g.t=_.cache(_.process(g.ctoken2("shortMeridian"),t.meridian));g.tt=_.cache(_.process(g.ctoken2("longMeridian"),t.meridian));g.z=_.cache(_.process(_.rtoken(/^(\+|\-)?\s*\d\d\d\d?/),t.timezone));g.zz=_.cache(_.process(_.rtoken(/^(\+|\-)\s*\d\d\d\d/),t.timezone));g.zzz=_.cache(_.process(g.ctoken2("timezone"),t.timezone));g.timeSuffix=_.each(_.ignore(g.whiteSpace),_.set([g.tt,g.zzz]));g.time=_.each(_.optional(_.ignore(_.stoken("T"))),g.hms,g.timeSuffix);g.d=_.cache(_.process(_.each(_.rtoken(/^([0-2]\d|3[0-1]|\d)/),_.optional(g.ctoken2("ordinalSuffix"))),t.day));g.dd=_.cache(_.process(_.each(_.rtoken(/^([0-2]\d|3[0-1])/),_.optional(g.ctoken2("ordinalSuffix"))),t.day));g.ddd=g.dddd=_.cache(_.process(g.ctoken("sun mon tue wed thu fri sat"),function(s){return function(){this.weekday=s;};}));g.M=_.cache(_.process(_.rtoken(/^(1[0-2]|0\d|\d)/),t.month));g.MM=_.cache(_.process(_.rtoken(/^(1[0-2]|0\d)/),t.month));g.MMM=g.MMMM=_.cache(_.process(g.ctoken("jan feb mar apr may jun jul aug sep oct nov dec"),t.month));g.y=_.cache(_.process(_.rtoken(/^(\d\d?)/),t.year));g.yy=_.cache(_.process(_.rtoken(/^(\d\d)/),t.year));g.yyy=_.cache(_.process(_.rtoken(/^(\d\d?\d?\d?)/),t.year));g.yyyy=_.cache(_.process(_.rtoken(/^(\d\d\d\d)/),t.year));_fn=function(){return _.each(_.any.apply(null,arguments),_.not(g.ctoken2("timeContext")));};g.day=_fn(g.d,g.dd);g.month=_fn(g.M,g.MMM);g.year=_fn(g.yyyy,g.yy);g.orientation=_.process(g.ctoken("past future"),function(s){return function(){this.orient=s;};});g.operator=_.process(g.ctoken("add subtract"),function(s){return function(){this.operator=s;};});g.rday=_.process(g.ctoken("yesterday tomorrow today now"),t.rday);g.unit=_.process(g.ctoken("minute hour day week month year"),function(s){return function(){this.unit=s;};});g.value=_.process(_.rtoken(/^\d\d?(st|nd|rd|th)?/),function(s){return function(){this.value=s.replace(/\D/g,"");};});g.expression=_.set([g.rday,g.operator,g.value,g.unit,g.orientation,g.ddd,g.MMM]);_fn=function(){return _.set(arguments,g.datePartDelimiter);};g.mdy=_fn(g.ddd,g.month,g.day,g.year);g.ymd=_fn(g.ddd,g.year,g.month,g.day);g.dmy=_fn(g.ddd,g.day,g.month,g.year);g.date=function(s){return((g[Date.CultureInfo.dateElementOrder]||g.mdy).call(this,s));};g.format=_.process(_.many(_.any(_.process(_.rtoken(/^(dd?d?d?|MM?M?M?|yy?y?y?|hh?|HH?|mm?|ss?|tt?|zz?z?)/),function(fmt){if(g[fmt]){return g[fmt];}else{throw Date.Parsing.Exception(fmt);}}),_.process(_.rtoken(/^[^dMyhHmstz]+/),function(s){return _.ignore(_.stoken(s));}))),function(rules){return _.process(_.each.apply(null,rules),t.finishExact);});var _F={};var _get=function(f){return _F[f]=(_F[f]||g.format(f)[0]);};g.formats=function(fx){if(fx instanceof Array){var rx=[];for(var i=0;i<fx.length;i++){rx.push(_get(fx[i]));}
return _.any.apply(null,rx);}else{return _get(fx);}};g._formats=g.formats(["yyyy-MM-ddTHH:mm:ss","ddd, MMM dd, yyyy H:mm:ss tt","ddd MMM d yyyy HH:mm:ss zzz","d"]);g._start=_.process(_.set([g.date,g.time,g.expression],g.generalDelimiter,g.whiteSpace),t.finish);g.start=function(s){try{var r=g._formats.call({},s);if(r[1].length===0){return r;}}catch(e){}
return g._start.call({},s);};}());Date._parse=Date.parse;Date.parse=function(s){var r=null;if(!s){return null;}
try{r=Date.Grammar.start.call({},s);}catch(e){return null;}
return((r[1].length===0)?r[0]:null);};Date.getParseFunction=function(fx){var fn=Date.Grammar.formats(fx);return function(s){var r=null;try{r=fn.call({},s);}catch(e){return null;}
return((r[1].length===0)?r[0]:null);};};Date.parseExact=function(s,fx){return Date.getParseFunction(fx)(s);};

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
//extensions
Number.prototype.format = function (n, x) {
    var re = '\\d(?=(\\d{' + (x || 3) + '})+' + (n > 0 ? '\\.' : '$') + ')';
    return this.toFixed(Math.max(0, ~~n)).replace(new RegExp(re, 'g'), '$&,');
};
Number.prototype.formatMoney = function (places, symbol, thousand, decimal) {
    places = !isNaN(places = Math.abs(places)) ? places : 2;
    symbol = symbol !== undefined ? symbol : "$";
    thousand = thousand || ",";
    decimal = decimal || ".";
    var number = this,
	    negative = number < 0 ? "-" : "",
	    i = parseInt(number = Math.abs(+number || 0).toFixed(places), 10) + "",
	    j = (j = i.length) > 3 ? j % 3 : 0;
    return symbol + negative + (j ? i.substr(0, j) + thousand : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thousand) + (places ? decimal + Math.abs(number - i).toFixed(places).slice(2) : "");
};
String.prototype.removeFormatting = function () {
    return parseFloat(this.replace(/[\$,]/g, ''));
}
String.prototype.isEmpty = function () {
    if (this == null || this.trim() == "" || this.trim().length < 1)
        return true;
    return false;
}
$.widget("ui.dialog", $.extend({}, $.ui.dialog.prototype, {
    _title: function (title) {
        if (!this.options.title) {
            title.html("&#160;");
        } else {
            title.html(this.options.title);
        }
    }
}));
$.validator.setDefaults({
    ignore: []
});

//global variables
var actsTempStorage = null;
var matsTempStorage = null;
var actsSelectedValue = [];
var temp;
var wochanged = false;
var dTable;
var aDtable;
var techTable;
var requiresNew = true;
var additional;
var additionalFunc;
var batch = false;
var deleteUrl = null;
var canEdit = 'true';
var isWO = false;
var isMatAddedToist = false;
var tempActs = [];
var tempLoc = "";
var tempLocs = [];
var tempDate = null;
var actMaterialQtyArry = [];
var dialogOptions = {
    dialogClass: "animated tada",
    width: 300,
    resizable: false,
    modal: true,
    close: function (event, ui) {
        $(this).remove();
    },
    buttons: [{ text: "OK", click: function () { $(this).dialog("close"); } }]
};
var dataTableOptions = {
    dom: '<"top"<"sub-top clearfix"fr>lp>t<"bottom clearfix"p>',
    pagingType: 'full_numbers',
    "autoWidth": false,
    "language": { "lengthMenu": "_MENU_" },
    "search": { "caseInsensitive": true },
    processing: true,
    paging: true,
    stateSave: true,
    serverSide: true,
    columnDefs: null,
    createdRow: null,
    order: null,
    langauge: {
        "processing": ""
    },
    ajax: {
        url: "", type: "POST",
        data: function (d) {
            d.additional = $("#status").val();
            d.year = $("#year").val();
            d.client = $("#clients").val();
            d.zone = $("#zone").val();
            d.clas = $("#clas").val();
            return $.toDictionary(d);
        }
    },
    drawCallback: function (settings) {
        $("[title]:not(.ui-dialog-titlebar [title])").tooltip({ placement: "top" });
    },
    preDrawCallback: function (e) {
        
    },
    footerCallback: null,
    initComplete: function (settings, json) {
        if (requiresNew)
            $(".dataTables_wrapper .sub-top").append("<button class='add-new'>New</button>");
        $(".dataTables_wrapper .top select").select2({ minimumResultsForSearch: -1, width: "resolve" });
        
    }
};

$(window).load(function () {
    
    //contractor statements
    $(document).on("click", ".manage-dets.tt-details, .manage-dets.tt-print", function (e) {
        loading(false);
        var self = $(this);
        var id = $(".tech-tbl").data("id");
        var pdate = self.parents("tr").attr("id");
        var url = "../technician/" + (self.hasClass("tt-details") ? "statementdetail" : "perioddetail");
        $.get(url, { id: id, pdate: pdate }, function (html) {
            removeLoader(false, 500, function () {
                $(".overlay-body").append(html);
                $(".overlay").addClass("everything").show(200, function () {
                    var olay = $(this);
                    setTimeout(function () {
                        olay.perfectScrollbar();
                    }, 200);
                });
            });
        });
        e.preventDefault();
    });
    $(document).on("click", ".manage-dets.tt-details, .manage-dets.tt-print-payslip", function (e) {
        //loading(false);
        var self = $(this);
        var id = $(".tech-tbl").data("id");
        var pdate = self.parents("tr").attr("id");
        var url = "../technician/GenerateTechPaySlipForPeriod";
        $.post(url, { id: id, pdate: pdate }, function (html) {
            removeLoader(false, 500, function () {
                //$(".overlay-body").append(html);
                //$(".overlay").addClass("everything").show(200, function () {
                //    var olay = $(this);
                //    setTimeout(function () {
                //        olay.perfectScrollbar();
                //    }, 200);
                //});
            });
        });
        e.preventDefault();
    });
    $(document).on("click", ".input-daterange .refresh", function () {
        techTable.draw(false);
    });
    $(document).on("click", ".input-daterange .cancel", function () {
        $(".input-daterange input").val("");
        $('.dataTables_wrapper .sub-top .input-daterange').datepicker('remove').datepicker({
            format: "DD M dd, yyyy"
        });
        techTable.fnDraw();
    });
    //print stuff
    $(document).on("click", ".print-holder .print-clicker", function (e) {
        window.print();
        e.preventDefault();
    });
    //user stuff 
    $(document).on("click", "#nav-password", function (e) {
        $.get("../auth/changepassword", function (html) {
            $("body").append(html);
            $('.modal').modal({ show: true });
            $.validator.unobtrusive.parse($(".tel-form form"));
        });
        e.preventDefault();
    });
    //print stuff
    $(document).on("click", ".hide-close", function () {
        var overlay = this.parentNode;
        if (overlay.id != undefined && overlay.id == "drillDown") {
            $("#drillDown").removeClass("everything report").hide(200).find(".overlay-body").html("");
            $(".overlay").show();
            if (batch == true) {
                var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); batchList() } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
                ];
                var msg = '<p>Do you want to <b class="default-col">Batch</b> the currently selected <b>Invoices</b>?</p></div>';
                displayConfirmationMsg(msg, 'Batching', buttons);
            }
        }
        else {
            $(".overlay").removeClass("everything report").hide(200).find(".overlay-body").html("");
            if (batch == true) {
                var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); batchList() } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
                ];
                var msg = '<p>Do you want to <b class="default-col">Batch</b> the currently selected <b>Invoices</b>?</p></div>';
                displayConfirmationMsg(msg, 'Batching', buttons);
            }
        }
    });
    $(document).on("click", ".print-holder .fk-checkbox", function (e) {
        var self = $(this);
        self.toggleClass("checked");
    });
    //datatable stuff
    $(document).on("click", ".dt-delete", function (e) {
        var self = $(this);
        var contractorName = $("#liContractorName").html() != undefined ? (" " + $("#liContractorName").html()) : '';
        var itemName = self.parents("tr").find(".dt-item-title").text() + contractorName;
        var itemType = self.parents("table").data("name");
        var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); deleteRow(self); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
        ];
        var msg = '<p>Are you sure you want to <b class="default-col">Delete</b> <b>' + itemName + '</b> record?</p></div>';
        displayConfirmationMsg(msg, 'Delete ' + itemType, buttons);
        e.preventDefault();
    });

    $(document).on("click", ".dt-delete-swo", function (e) {
        var self = $(this);
        var contractorName = $("#liContractorName").html() != undefined ? (" " + $("#liContractorName").html()) : '';
        var itemName = self.parents("tr").find(".dt-item-title").text() + contractorName;
        var itemType = self.parents("table").data("name");
        var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); deleteSWORow(self); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
        ];
        var msg = '<p>Are you sure you want to <b class="default-col">Delete</b> <b>' + itemName + '</b> record?</p></div>';
        displayConfirmationMsg(msg, 'Delete ' + itemType, buttons);
        e.preventDefault();
    });
    $(document).on("click", ".dt-edit", function (e) {
        var self = $(this);
        var row = self.parents("tr");
        var canContinue = true;
        if (isWO) {
            var elm = row[0].cells[3].children[0] != undefined ? row[0].cells[3].children[0].className : "";            
            if (elm == "fe-ok yes" && (canEdit == "False" || canEdit == "false"))
                canContinue = false;
        }
        if (canContinue) {
            loading(false);
            var url = $(".admin-tbl").data("url") + "/edit";
            var id = $(this).parents("tr").attr("id");
            id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
            $.get(url, { id: id }, function (html) {
                removeLoader(false, 500, function () {
                    $("body").append(html);
                    $('.modal').modal({ show: true }).find("select").select2({ minimumResultsForSearch: -1, width: "resolve" });
                    $('.modal').find(".number").onlyNumbers({ neagtive: false, decimal: true });
                    $('.modal .date-box').datepicker({ format: "mm/dd/yyyy" }).on('changeDate', function (ev) {
                        $(this).datepicker('hide');
                    });                    
                    $.validator.unobtrusive.parse($(".tel-form form"));
                });
            });
        }
        else {
            var htmlMsg = "<p>This Workorder has already been <b>Submitted</b>.</p>";
            var htmlTitle = "<i class='fe-comment'></i>Already Submitted";
            displayDialogMsg(htmlMsg, htmlTitle);
        }
        e.preventDefault();
    });
    $(document).on("click", ".dt-edit-swo", function (e) {
        loading(false);
        //var url = $(".admin-tbl").data("url") + "/EditSWO";
        var id = $(this).parents("tr").attr("id");
        id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
        window.open("/workorder/EditSWO?id=" + id, "_self");

        //$.get(url, { id: id }, function (html) {
        //    removeLoader(false, 500, function () {
        //        if (data.Additional > 0) {
        //            $(".tel-confirm-dialog").dialog("close");
        //            dTable.draw();
        //        }
        //        else {
        //            $(".tel-confirm-dialog").dialog("close");
        //            var htmlMsg = "<p>This record cannot be <b class='default-col'>Edited</b> because it is <b>not in</b> New status.</p>";
        //            var htmlTitle = "<i class='fe-comment'></i>Cannot Edit";
        //            displayDialogMsg(htmlMsg, htmlTitle);
        //        }
        ////        $("body").append(html);
        ////        $('.modal').modal({ show: true }).find("select").select2({ minimumResultsForSearch: -1, width: "resolve" });
        ////        $('.modal').find(".number").onlyNumbers({ neagtive: false, decimal: true });
        ////        $('.modal .date-box').datepicker({ format: "mm/dd/yyyy" });
        ////        $.validator.unobtrusive.parse($(".tel-form form"));
        //    });
        //});
        e.preventDefault();
    });
    $(document).on("click", ".dt-condeduction", function (e) {
        var url = "/contractordeduction/index?p_ConId=";
        var id = $(this).parents("tr").attr("id");
        id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
        window.location.href = url + id;
    });
    $(document).on("click", ".dt-conpayslip", function (e) {
        var url = "/technician/techpayslip?p_ConId=";
        var id = $(this).parents("tr").attr("id");
        id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
        window.location.href = url + id;
    });
    //form stuff
    $(document).on("click", ".dataTables_wrapper .sub-top .add-new, .dataTables_wrapper .sub-top .dropdown-menu>li>a", function (e) {
        var self = $(this);
        var url = self.is(".add-new") ? $(".admin-tbl, .tech-tbl").data("url") + "/create" : self.attr("href");
        url += typeof self.data("type") === 'undefined' ? "" : "?type=" + self.data("type");
        loading(true);
        $.get(url, function (html) {
            removeLoader(true, 200, function () {
                $("body").append(html);
                $('.modal').modal({ show: true }).find("select").select2({ minimumResultsForSearch: -1, width: "resolve", placeholder: "Select an option" });
                $('.modal').find(".number").onlyNumbers({ neagtive: false, decimal: true });
                $('.modal .date-box').datepicker({ format: "mm/dd/yyyy" }).on('changeDate', function (ev) {
                    $(this).datepicker('hide');
                });
                
                $.validator.unobtrusive.parse($(".tel-form form"));
            });
            setTimeout('$($(".long")[0]).focus()', 1500);
        });
        e.preventDefault();
    });
    $(document).on("click", ".dataTables_wrapper .sub-top .add-new-swo", function (e) {
        loading(false);
        window.open("/workorder/createNewSWO", "_self");
        e.preventDefault();
    });
    $(document).on("submit", ".tel-form form", function (e) {
        var self = $(this);
        if (isWO) {
            SubmitWO(self, false);
            e.preventDefault();
            return;
        }
        self.validate();
        var data = new FormData(self.get(0));
        if (self.valid()) {
            $(".tel-form button.saver, .tel-form input, tel-form select").attr("disabled", "disabled");
            $.ajax({
                url: self.attr("action"),
                data: data,
                type: "post",
                cache: false,
                contentType: false,
                processData: false,
                success: function (data) {
                    $(".tel-form button.saver, .tel-form input, tel-form select").removeAttr("disabled");
                    if (data.Code != "100") {
                        if (data.Code == "999")
                            setupMsgBar(3, data.Additional);
                        else if(data.Code == '1436')
                            setupMsgBar(3, "Create/Update failed. You don't have permission to perform this action.");
                        else
                            setupMsgBar(3, "Create/Update failed. Please try again or contact your system administrator");
                    }
                    else {
                        setupMsgBar(2, "Create/Update was completed successfully!");
                        if (data.Additional == "" || data.Additional.length < 0 || data.Additional == null) {
                            $(".tel-form input").val("");
                            var page = dTable.page();
                            dTable.page(page).draw(false);
                            //dTable.draw();
                            setTimeout('$(".close").click()', 2000);
                        }
                        else if (data.Additional.toString().substr(0, 1) == "#")
                            setupMsgBar(3, data.Additional.substr(1));
                        else if (typeof data.Additional === 'string' || !isNaN(data.Additional)) {
                            $(".tel-form .tel-form-id").val(data.Additional);
                            if (!isWO) {
                                var page = dTable.page();
                                dTable.page(page).draw(false);
                                //dTable.draw();
                                setTimeout('$(".close").click()', 2000);
                            }
                            else {
                                var self = $("#teams");
                                var btns = $(".tel-form .tel-tabs button");
                                btns.removeClass("active");
                                self.addClass("active");
                                $(".modal-footer .update").removeClass("team-btn link-btn");
                                $(".tel-form table.active").removeClass("active").hide(200, function () {
                                    $(".tel-form table").eq(1).addClass("active").show(200);
                                    if (self.is("#teams")) {
                                        setupTeamActions(true);
                                        $(".modal-footer .update").toggleClass("saver team-btn");
                                    }
                                });
                            }
                            isWO = false;
                        }
                        else {
                            for (var prop in data.Additional) {
                                $(".tel-form [name='" + prop + "']").val(data.Additional[prop]);
                            }
                            var page = dTable.page();
                            dTable.page(page).draw(false);
                            //dTable.draw();
                            setTimeout('$(".close").click()', 2000);
                        }
                        $(".tel-form .tel-tabs button").removeAttr("disabled");
                    }
                }
            });
        }
        e.preventDefault();
    });
    function SubmitWO(self, fromTeam) {
        self = $("#wOForm");
        self.validate();
        var data = new FormData(self.get(0));
        if (self.valid()) {
            $(".tel-form button.saver, .tel-form input, tel-form select").attr("disabled", "disabled");
            $.ajax({
                url: self.attr("action"),
                data: data,
                type: "post",
                cache: false,
                contentType: false,
                processData: false,
                success: function (data) {
                    $(".tel-form button.saver, .tel-form input, tel-form select").removeAttr("disabled");
                    if (data.Code != "100") {
                        if (data.Code == "999")
                            setupMsgBar(3, data.Additional);
                        else
                            setupMsgBar(3, "Create/Update failed. Please try again or contact your system administrator");
                    }
                    else {
                        setupMsgBar(2, "Create/Update was completed successfully!");
                        if (data.Additional == "" || data.Additional.length < 0 || data.Additional == null)
                            $(".tel-form input").val("");
                        else if (data.Additional.toString().substr(0, 1) == "#")
                            setupMsgBar(3, data.Additional.substr(1));
                        else if (typeof data.Additional === 'string' || !isNaN(data.Additional)) {
                            $(".tel-form .tel-form-id").val(data.Additional);
                            if (!fromTeam) {
                                var self = $("#teams");
                                var btns = $(".tel-form .tel-tabs button");
                                btns.removeClass("active");
                                self.addClass("active");
                                $(".modal-footer .update").removeClass("team-btn link-btn");
                                $(".tel-form table.active").removeClass("active").hide(200, function () {
                                    $(".tel-form table").eq(1).addClass("active").show(200);
                                    if (self.is("#teams")) {
                                        setupTeamActions(true);
                                        $(".modal-footer .update").toggleClass("saver team-btn");
                                    }
                                });
                            }
                            else {
                                setTimeout('$(".close").click()', 2000);
                            }
                            isWO = false;
                        }
                        else {
                            for (var prop in data.Additional) {
                                $(".tel-form [name='" + prop + "']").val(data.Additional[prop]);
                            }
                            var page = dTable.page();
                            dTable.page(page).draw(false);
                            setTimeout('$(".close").click()', 2000);
                        }
                        $(".tel-form .tel-tabs button").removeAttr("disabled");
                    }
                }
            });
        }
        //e.preventDefault();
    }
    $(document).on("click", ".tel-form button.saver", function (e) {
        //if (isWO) {
        //    //SaveWO();
        //}
        //else {
            var form = $(".tel-form form");
            form.submit();
            setValidationMsgsForForm(form);
        //}
    });
    $(document).on("keyup, change", ".tel-form form td > input, .tel-form form div > input", function (e) {
        var self = $(this);
        if (!self.valid())
            setValidationMsgsForElm(self);
        else
            self.tooltip("destroy");
    });
    $(document).on("click", ".tel-checkbox", function (e) {

        var self = $(this);
        self.toggleClass("checked");
        self.find(":hidden").val(self.is(".checked") ? true : false);
        e.preventDefault();
    });
    $(document).on("click", ".tel-radio", function (e) {
        var self = $(this);
        var name = self.find(":hidden").attr("name");
        $(".tel-radio").not(self).removeClass("checked").find(":hidden").val(false);
        self.toggleClass("checked");
        self.find(":hidden").val(self.is(".checked") ? true : false);
        e.preventDefault();
    });

    //the hard stuff
    //constructor
    $(document).on("click", ".const-main .const-holder ul li > .wo-actions .add-materials button", function (e) {
        console.log("asfasf")
        var self = $(this);
        self.toggleClass("off");
        var parent = self.parents("li").find(".wo-actions");
        var matActn = parent.find(".mat-action");
        if (self[0].classList.length == 0) {
            LoadMetarialUsagePopup(self);            
            if (matActn.length < 1) {
                parent.append('<a href="#" class="mat-action"><i class="mdi mdi-server" "></i></a>');//style="color: green;
            }
            else
                $(matActn[0]).show();
        }
        else {
            if (!(matActn.length < 1)) {
                $(matActn[0]).hide();
            }
        }
        e.preventDefault();
    });
    $(document).on("click", "#constructor .title-editor button", function (e) {
        var self = $(this);
        self.parent().find("input").animate({ width: self.hasClass("active") ? 0 : 300 }, 200, "easeInOutBack", function () {
            self.toggleClass("active");
            $(this).toggleClass("active");
        });
        e.preventDefault();
    });
    $(document).on("click", ".has-constructor .dt-details, #con-wo .dt-details", function (e) {
        var self = $(this);
        var wid = self.parents("tr").attr("id");
        wid = wid.toLowerCase().indexOf("invoiced") > -1 ? "invoiced" : wid.split("_")[1];
        if (wid != "invoiced") {
            loading(false);
            url = self.parents("table").data("wurl");
            $("body").append("<div class='full-overlay'></div>");
            $(".full-overlay").fadeIn(200);
            $.ajax({
                url: url,
                data: { wid: wid },
                type: "POST",
                success: function (html) {
                    removeLoader(false, 500, function () {
                        $("body").append(html);
                        $("#constructor [title]").tooltip({ placement: "top" });
                        $("#constructor").addClass("animated bounceInRight").show();
                        $('.const-main .const-holder ul').perfectScrollbar();
                        $("#constructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                        $("#constructor .title span").dotdotdot();
                    });
                }
            });
        }
        else {
            var htmlMsg = "<p>This Workorder has already been <b>Invoiced</b>. To edit the Workorder you must first reverse the Invoice.</p>";
            var htmlTitle = "<i class='fe-comment'></i>Already Processed";
            displayDialogMsg(htmlMsg, htmlTitle);
        }
        e.preventDefault();
    });

    //constrcutor items
    $(document).on("click", "#constructor .const-holder.const-dates ul li:not(.active)", function (e) {
        var stop = discardDataCheck("acts");
        temp = $(this);
        if (tempDate != null) {
            var activeDate = tempDate;            
            $(".const-main .const-holder.const-locs ul li").each(function () {
                var self = $(this);
                var activeLoc = $(self.find("span")[0]).text();
                if (!CheckLocInTheArry(activeLoc, activeDate)) {
                    tempLocs.push({
                        location: activeLoc,
                        actDate: activeDate,
                        comment: $(self.find("span")[1]).text(),
                    });
                }
            });
        }

        //if (stop) {
        //    var buttons = [
        //        { text: "Continue", click: function () { $(this).dialog("close"); generateLocations(); } },
        //        { text: "Cancel", click: function () { $(this).dialog("close"); } }
        //    ];
        //    var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
        //    displayConfirmationMsg(msg, 'Discard Changes', buttons);
        //}
        //else {
            generateLocations();
        //}
            tempDate = temp.data("date");
    });
    
    $(document).on("click", "#constructor .const-holder.const-locs ul li", function (e) {
        
        var stop = discardDataCheck("locs");
        temp = $(this);
        //if (stop) {
            //var buttons = [
            //    { text: "Continue", click: function () { $(this).dialog("close"); generateActivities(); } },
            //    { text: "Cancel", click: function () { $(this).dialog("close"); } }
            //];
            //var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
            //displayConfirmationMsg(msg, 'Discard Changes', buttons);

            //$("#constructor .const-locs ul li.new-loc, #constructor .const-acts ul li.new-act").length > 0
            
            if (tempLoc != "") {
                var activeDate = $("#constructor .const-holder.const-dates ul li.active").data("date");
                var activeLoc = tempLoc;
                $(".const-main .const-holder.const-acts ul li").each(function () {
                    var self = $(this);
                    var iD = self.data("id");
                    var actID = self.find("select").val();
                    var actQty = self.find(".act-amt").val();
                    var matsReq = self.find(".mats-req") != undefined ? self.find(".mats-req").val() : false;
                    if (!actID.isEmpty() && !actQty.isEmpty() && !CheckActInTheArry(actID, activeLoc, activeDate)) {
                        tempActs.push({
                            id: iD,
                            location: activeLoc,
                            actDate: activeDate,
                            actID: actID,
                            actQty: actQty,
                            matsReq: matsReq
                        });
                    }
                });
            }
            generateActivities();
        //}
        //else {
        //    generateActivities();
        //}
        //var actLoc = $($(".const-main .const-holder.const-locs ul li.active").find("span")[0]).text();
        tempLoc = $(temp.find("span")[0]).text();
    });
    function CheckActInTheArry(actId, loc, date) {
        var inArry = false;
        for (var i = 0; i < tempActs.length; i++) {
            if (tempActs[i].actID == actId && tempActs[i].location == loc && tempActs[i].actDate == date) {
                inArry = true;
                break;
            }
        }
        return inArry;
    }
    function CheckLocInTheArry(loc, date) {
        var inArry = false;
        for (var i = 0; i < tempLocs.length; i++) {
            if (tempLocs[i].location == loc && tempLocs[i].actDate == date) {
                inArry = true;
                break;
            }
        }
        return inArry;
    }
    //constructor item actions
    $(document).on("click", "#constructor .const-dates > .title > a, #constructor .const-dates .placeholder button", function (e) {
        var self = $(this);
        setupActivityCalendar(null);
        e.preventDefault();
    });
    $(document).on("click", "#constructor .const-locs > .title > a, #constructor .const-locs .placeholder button", function (e) {
        
        if ($("#constructor .const-dates ul li.active").length == 0) {
            var msg = "<p>Please add and/or select a <b>Date</b> before attempting to add any new <b>Locations</b>.</p>";
            var title = "<i class='fe-comment'></i> Cannot Add Location";
            displayDialogMsg(msg, title);
        }
        else {
            var self = $(this);
            setupLocationDialogBox("", "");
        }
        e.preventDefault();
    });
    $(document).on("click", "#constructor .const-acts > .title > a, #constructor .const-acts .placeholder button", function (e) {
        
        if ($("#constructor .const-locs ul li.active").length == 0) {
            var msg = "<p>Please add and/or select a <b>Location</b> before attempting to add any new <b>Activities</b>.</p>";
            var title = "<i class='fe-comment'></i> Cannot Add Activity";
            displayDialogMsg(msg, title);
        }
        else {
            var actsHolder = $("#constructor > .const-main .const-acts");
            var self = $(this);
            if (actsHolder.find("ul").length == 0)
                actsHolder.append("<ul></ul>");
            if (actsTempStorage == null) {
                $.ajax({
                    async: false,
                    url: '/workorder/getactivities',
                    data: { wid: $("#constructor").data("id") },
                    type: 'post',
                    success: function (r) {
                        actsTempStorage = r;
                    }
                })
            }
            var ph = $("#constructor .const-acts .placeholder");
            var time = ph.is(":visible") ? 300 : 10;
            ph.hide(200);
            setTimeout(function () {
                actsHolder.find("ul").append(applyActivityTemplate(actsTempStorage));
                $('.const-main .const-acts ul').perfectScrollbar("update");
                var n = $('.const-main .const-acts ul').height();
                $('.const-main .const-acts ul').animate({ scrollTop: n }, 50);

                setDisableActivityOptions();
                actsHolder.find("select").select2({ width: "resolve", placeholder: "Select an Activity" });
                actsHolder.find("input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                //actsHolder.find("input.act-amt").on("blur", function (e) {
                //    var self = $(this);
                //    var prnt = $(this.parentElement);
                //    var id = prnt.find("select").val();
                //    $.post("/workorder/GetMaxQty", { actId: id }, function (r) {
                //        var msg = "";
                //        var title = "<i class='fe-comment'></i>Quntity Warning!"
                //        if (r.MaxQty != 0) {
                //            if (Number(self.val()) > r.MaxQty)
                //                msg = "Quntity cannot be greater than " + r.MaxQty + ".";
                //        }
                //        if (Number(self.val()) < 1)
                //            msg = "Quntity cannot be 0 or minus value.";
                //        if (msg != "") {
                //            displayDialogMsg(msg, title);
                //            self.val(1.00);
                //        }
                //    });
                //});
            }, time);
        }
        e.preventDefault();
    });
    $(document).on("change", "#constructor .const-acts select", function () {
        var self = $(this);
        var id = self.val();
        temp = self.parents("li");
        setDisableActivityOptions();
        $.get("/workorder/hasmaterial", { id: id }, function (result) {
            var parent = self.parents("li").find(".wo-actions");
            if (result.hasMats == true) {
                if (parent.find(".mat-action").length < 1) {
                    var str = '<div class="add-materials-contn">'
                                + '<div class="add-materials">'
                                + '<label>'
                                //+ '<span>Materials</span>'
                                //+ '<button>'
                                //+ '<span></span>'
                                //+ '</button>'
                                //+ '<input type="hidden" value="" />'
                                + '</label>'
                                + '</div>'
                                + '</div>';
                    parent.append(str + '<input type="hidden" class="mats-req" value="' + result.matsReq + '"/><a href="#" class="mat-action" title="Materials"><i class="mdi mdi-server"></i></a>');// style="color: green;"
                    //LoadMaterialPopup(temp);
                }
            }
            else {
                parent.find(".mat-action").remove();
            }
        });
    });

    //constructor top btns
    $(document).on("click", "#constructor > .title .btn-group label:not(.active)", function (e) {
        var self = $(this);
        var selfCls = self.attr("class");
        var type = selfCls.indexOf("unsubmit") > -1 ? "unsubmit" : selfCls.indexOf("verify") > -1 ? "verify" : "submit";
        var canContinue = true;
        if (type == "verify") {
            canContinue = false;
            CheckMaxQtyExceeded();
        }
        if (canContinue) {
            ChangeWOStatus(type);
        }
    });
    function CheckMaxQtyExceeded() {
        var id = $("#constructor").data("id");
        $.post("/workorder/CheckMaxQtyExceeded", { id: id }, function (r) {
            if (r.msg != "") {
                var buttons = [
                        { text: "Continue", click: function () { CheckAuthentication(this) } },
                        {
                            text: "Cancel", click: function () {
                                $(this).dialog("close");
                                $("#constructor > .title .btn-group label").removeClass("active");
                                $("#constructor > .title .btn-group label.submit").addClass("active");
                            }
                        }
                ];
                var msg = "<div><p>Cannot change the status, some <b class='default-col'>Activities</b> or <b class='default-col'>Materials</b> maximum quantity <b class='default-col'>Exceeded</b>.</p>" +
                    "<br />" +
                    "<div class='txt-holder'>"+
                    "<input type='text' placeholder='Username' style='height:30px;' id='txtUsername' />"+
                    "<i class='fe-user'></i>"+
                    "</div>" +
                    //"<br />" +
                    "<div class='txt-holder'>"+
                    "<input type='password' placeholder='Password' style='height:30px; margin-top:5px;' id='txtPassword' />" +
                    "<i class='fe-lock-open'></i>"+
                    "</div>"+
                    "</div>";
                displayConfirmationMsg(msg, 'Authentication Required', buttons);                
            }
            else
                ChangeWOStatus("verify");
        });
    }
    function ChangeWOStatus(type) {
        $.post("/workorder/changeStatus", { type: type, wid: $("#constructor").data("id") }, function (r) {
            if (r.Msg != "") {
                $("#constructor > .title .btn-group label").removeClass("active");
                if (type == "submit")
                    $("#constructor > .title .btn-group label.submit").addClass("active");
                else if (type == "verify")
                    $("#constructor > .title .btn-group label.verify").addClass("active");
                else
                    $("#constructor > .title .btn-group label.unsubmit").addClass("active");
                var msg = r.Msg;
                var title = "<i class='fe-comment'></i> Cannot Change Status!"
                if (r.Code == "1001")
                    title = "<i class='fe-comment'></i>Penalty Warning!"
                displayDialogMsg(msg, title);
            }
        });
    }
    function CheckAuthentication(e) {
        //$(this).dialog("close");
        var self = $(e);
        var uName = self.find("#txtUsername").val();
        var pWord = self.find("#txtPassword").val();
        if (uName == "" || pWord == "")
            return;
        $.post("/workorder/CheckAuthentication", { uName: uName, pWord:pWord }, function (r) {
            if (r.msg != "") {
                self.dialog("close");
                var buttons = [
                        { text: "Continue", click: function () { CheckAuthentication(this) } },
                        {
                            text: "Cancel", click: function () {
                                $(this).dialog("close");
                                $("#constructor > .title .btn-group label").removeClass("active");
                                $("#constructor > .title .btn-group label.submit").addClass("active");
                            }
                        }
                ];
                var msg = "<div><p>Cannot change the status, some <b class='default-col'>Activities</b> or <b class='default-col'>Materials</b> maximum quantity <b class='default-col'>Exceeded</b>.</p>" +
                    "<br />" +
                    "<div class='txt-holder'>" +
                    "<input type='text' placeholder='Username' style='height:30px;' id='txtUsername' />" +
                    "<i class='fe-user'></i>" +
                    "</div>" +
                    //"<br />" +
                    "<div class='txt-holder'>" +
                    "<input type='password' placeholder='Password' style='height:30px; margin-top:5px;' id='txtPassword' />" +
                    "<i class='fe-lock-open'></i>" +
                    "</div>" +
                    "</div>";
                displayConfirmationMsg(msg, 'Authentication Required', buttons);
            }
            else {
                self.dialog("close");
                ChangeWOStatus("verify");
            }
        });
    }
    //constructor edit and delete actions
    $(document).on("click", ".const-main .const-holder ul li .edit-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        var type = self.parents(".const-holder").hasClass("const-dates") ? "Date" : "Location";
        switch (type) {
            case "Date":
                setupActivityCalendar(self.parents("li").data("date"));
                break;
            default:
                setupLocationDialogBox($(self.parents("li").find("span")[0]).text(), $(self.parents("li").find("span")[1]).text());
                break;
        }
        e.stopPropagation();
        e.preventDefault();
    });
    $(document).on("click", ".const-main .const-holder ul li .del-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        if (temp.attr("class").indexOf("new") > -1) {
            temp.remove();
        }
        else {
            var holder = self.parents(".const-holder");
            var type = holder.hasClass("const-dates") ? "Date" : holder.hasClass("const-locs") ? "Location" : "Activity";
            var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); deleteAction(type); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msgAddl = "";
            if (type == "Date")
                msgAddl = "This action will also <b class='default-col'>Delete</b> all <b class='loc-col'>Locations</b>, <b class='act-col'>Activities</b> and <b class='act-col'>Materials</b> related to this <b class='date-col'>Date</b>.";
            else if (type == "Location")
                msgAddl = "This action will also <b class='default-col'>Delete</b> all <b class='act-col'>Activities</b> and <b class='act-col'>Materials</b> related to this <b class='loc-col'>Location</b>.";
            else
                msgAddl = "This action will also <b class='default-col'>Delete</b> all <b class='act-col'>Materials</b> related to this <b class='act-col'>Activity</b>."
            var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Delete ' + type, buttons);
        }
        e.stopPropagation();
        e.preventDefault();
    });
    $(document).on("click", ".const-main .const-holder ul li .mat-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        LoadMaterialPopup(temp);
        e.preventDefault();
    });
    function LoadMaterialPopup(temp) {
        var date = $("#constructor .const-holder.const-dates ul li.active").data("date");
        var loc = $($(".const-main .const-holder.const-locs ul li.active").find("span")[0]).text();
        var actId = temp.find("select").val();
        var wActId = $(temp).data("id");
        var id = $("#constructor").data("id");
        var invID = $("#constructor .wo-inv select").val();
        var type = $("#constructor").hasClass("view-wo") ? "view" : "";
        var qtyt = temp.find(".act-amt").val();
        var qty = qtyt != undefined && qtyt != "" ? Number(qtyt) : qtyt;
        loading(false);
        $("body").append("<div class='full-overlay'></div>");
        $(".full-overlay").fadeIn(200);
        $.ajax({
            url: "/WorkOrder/GenerateMetarialConstructor",
            data: { wActid: wActId, date: date, loc: loc, actId: actId, invID: invID, type: type, wid: id, qty: qty },
            type: "POST",
            success: function (html) {
                removeLoader(false, 500, function () {
                    isMatAddedToist = false;
                    $("body").append(html);
                    $("#matConstructor [title]").tooltip({ placement: "top" });
                    $("#matConstructor").addClass("animated bounceInRight").show();
                    $('.mat-const-main .const-holder ul').perfectScrollbar();
                    $("#matConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                    $("#matConstructor .title span").dotdotdot();
                    var matsHolder = $("#matConstructor > .mat-const-main .const-holder.const-mats");
                    var ph = matsHolder.find(".placeholder");
                    var time = ph.is(":visible") ? 300 : 10;
                    ph.find("button").removeClass("activated animated flash");
                    ph.hide(200);
                    var hasExisting = false;
                    if (actMaterialQtyArry.length > 0) {
                        if (matsHolder.find("ul").length == 0)
                            matsHolder.append("<ul></ul>");
                        var actID = $("#matConstructor #actIdHidden").val();
                        var loc = $("#matConstructor #locHidden").val();
                        var date = $("#matConstructor #dateHidden").val();
                        for (var i = 0; i < actMaterialQtyArry.length; i++) {
                            if (actMaterialQtyArry[i].loc == loc && actMaterialQtyArry[i].date == date && actMaterialQtyArry[i].actID == actID) {
                                if (!CheckIsMatInPlaceHolder(actMaterialQtyArry[i].matID)) {
                                    matsHolder.find("ul").append(applyMaterialTemplateWithValues(matsTempStorage, actMaterialQtyArry[i].matID, actMaterialQtyArry[i].matQty));
                                    $('.mat-const-main .const-mats ul').perfectScrollbar();
                                    setDisableMetarialOptions();
                                    matsHolder.find("select").select2({ width: "resolve", placeholder: "Select an Activity" });
                                    hasExisting = true;
                                }
                                else {
                                    $(".mat-const-main .const-holder.const-mats ul li").each(function () {
                                        var self = $(this);
                                        var matID = self.find("select").val();
                                        if (actMaterialQtyArry[i].matID == matID) {
                                            self.find(".act-amt").val(actMaterialQtyArry[i].matQty)
                                        }
                                    });
                                }
                            }
                        }
                    }
                    if (matsHolder.find("li").length == 0 && !hasExisting) {
                        matsHolder.find("ul").remove();
                        $(".mat-const-main .const-mats .placeholder").show(100, function () {
                            $(".mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                        });
                    }
                    else {
                        setTimeout(function () {
                            setDisableMetarialOptions();
                            matsHolder.find("select").select2({ width: "resolve" });
                            matsHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                            $('.mat-const-main .const-mats ul').perfectScrollbar();
                        }, time);
                    }
                });
            },
            error: function (e) {
                var aasdasd = e;
            }
        });
    }
    function CheckIsMatInPlaceHolder(matId) {
        var inPlace = false;
        $(".mat-const-main .const-holder.const-mats ul li").each(function () {
            var self = $(this);
            var matID = self.find("select").val();
            if (matId == matID) {
                inPlace = true;
            }
        });
        return inPlace;
    }
    function applyMaterialTemplateWithValues(mats, matId, matQty) {

        var matBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
        for (x = 0; x < mats.length; x++) {
            var mat = mats[x];
            matBox += '<option value="' + mat.activityID + '" ' + (mat.activityID == matId ? 'selected="selected"' : '') + '>' + mat.description + '</option>';
        }
        matBox += '</select><input type="text" class="act-amt" value="' + matQty + '" /></div>' +
            '<div class="wo-actions"><a href="#" class="del-action"><i class="fe-trash"></i></a></div></li>';
        return matBox;
    }

    $(document).on("blur", "#constructor .const-acts input.act-amt", function (e) {
        var self = $(this);
        temp = self.parents("li");
        var prnt = $(this.parentElement);
        var id = prnt.find("select").val();
        $.post("/workorder/GetMaxQty", { actId: id }, function (r) {
            var msgAddl = "";
            if (r.MaxQty != 0) {
                if (Number(self.val()) > r.MaxQty)
                    msgAddl = "You have exceed the <b class='default-col'>Maximum Quantity ( " + r.MaxQty + " )</b> level for the activity.";
            }
            if (Number(self.val()) < 1)
                msgAddl = "You are going to insert <b class='default-col'>Nagavite</b> value as activity quantity.";
            if (msgAddl != "") {
                var buttons = [
                        { text: "Continue", click: function () { $(this).dialog("close"); ContinueFun(r.hasMats) } },
                        { text: "Cancel", click: function () { $(this).dialog("close"); self.val(r.MaxQty); } }
                ];
                var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
                displayConfirmationMsg(msg, 'Quantity Warning', buttons);
            }
            else
                if (r.hasMats == true)
                    LoadMaterialPopup(temp);
        });
    });
    function ContinueFun(hasMats) {
        if (hasMats == true)
            LoadMaterialPopup(temp);
    }

    //constructor bottom btns
    $(document).on("click", "#constructor .process", function (e) {
        var id = $("#constructor").data("id");
        loading(true);
        $.get("../workorder/workorderactivitycount", { id: id }, function (data) {
            if (!data.any || !data.verified) {
                var title = "<i class='fe-comment'></i> Cannot Process Work Order!"
                var msg = "";
                removeLoader(true, 500, function () {
                    if (!data.any)
                        msg = "<p>Cannot <b class='default-col'>Process</b> a Work Order that has no activities. Please make sure that at least one (1) <b class='act-col'>Activity</b> was created.</p>";
                    else
                        msg = "<p>Cannot <b class='default-col'>Process</b> a Work Order that has not yet been <b class='act-col'>Verified</b>. Please make sure that you have changed the Work Order's status.</p>";
                    displayDialogMsg(msg, title);
                });
            }
            else {
                var invID = $("#constructor .wo-inv select").val();
                var title = $("#constructor .title-editor input").val();
                //var buttons = [
                //            {
                //                text: "Continue", click: function () {
                //                    $(this).dialog("close");
                                    loading(true);
                                    $.post("../workorder/processactivities", { id: id, invID: invID, title: title }, function (data) {
                                        removeLoader(true, 500, function () {
                                            if (typeof data.Additional === 'undefined') {
                                                closeConstructor();
                                                //$("#constructor").remove();
                                                //$("body").append(data);
                                                //$("#constructor").show();
                                            }
                                            msg = "<p>Successfully <b class='default-col'>Processed</b> Workorder. </p>";
                                            var title = "<i class='fe-comment'></i> Success!"
                                            displayDialogMsg(msg, title);
                                        });
                                    });
                //                }
                //            },
                //            { text: "Cancel", click: function () { $(this).dialog("close"); } }
                //];
                //var msg = '<p>Are you sure you want to <b class="default-col">Process</b> this Work Order?</p></div>';
                //removeLoader(true, 500, function () {
                //    displayConfirmationMsg(msg, 'Process Work Order', buttons);
                //});
            }
        });
        e.preventDefault();
    });
    $(document).on("click", "#constructor .cancel", function (e) {
        var stop = discardDataCheck("full");
        if (stop) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); closeConstructor(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
            displayConfirmationMsg(msg, 'Discard Changes', buttons);
        }
        else {
            closeConstructor();
        }
        e.preventDefault();
    });
    
    $(document).on("click", "#constructor .update", function (e) {
        
        var activeDate = $(".const-main .const-holder.const-dates ul li.active");
        var activeLoc = $(".const-main .const-holder.const-locs ul li.active");
        var acts = [];
        $(".const-main .const-holder.const-acts ul li").each(function () {
            var self = $(this);
            var actID = self.find("select").val();
            var actQty = self.find(".act-amt").val();
            if (!actID.isEmpty() && !actQty.isEmpty()) {
                acts.push({
                    actID: actID,
                    actQty: actQty
                });
            }
        });
        if (activeDate.length > 0 && activeLoc.length > 0 && acts.length > 0) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); updateAction(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = "<p>Only the selected <b class='date-col'>Date</b>, <b class='loc-col'>Location</b> and valid <b class='act-col'>Activities</b> will be <b class='default-col'>Updated</b>. Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Update Confirmation', buttons);
        }
        else {
            var msg = "<p>Cannot <b class='default-col'>Update</b> incomplete Workorder. Please make sure that a valid <b class='date-col'>Date</b> and <b class='loc-col'>Location</b> were select and that at least one (1) <b class='act-col'>Activity</b> was selected with an appropriate quantity.</p>";
            var title = "<i class='fe-comment'></i> Cannot Update Work Order!"
            displayDialogMsg(msg, title);
        }
        e.preventDefault()
    });
    // End constructor
    
    //Role Task Details

    $(document).on("click", ".dt-details-role", function (e) {
        var self = $(this);
        var id = self.parents("tr").attr("id");
        loading(false);
        url = self.parents("table").data("roleurl");
        $("body").append("<div class='btm-overlay'></div>");
        $(".btm-overlay").fadeIn(200);
        $.ajax({
            url: url,
            data: { id: id },
            type: "POST",
            success: function (html) {
                removeLoader(false, 500, function () {
                    $("body").append(html);
                    //$("#tasks [title]").tooltip({ placement: "top" });
                    $("#roletask .center").perfectScrollbar();
                });
            }
        });
        e.preventDefault();
    });
    $(document).on("click", "#roletask .tasks-holder td .check-box", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var tds = $("#roletask .tasks-holder td");
        var activeChecks = tds.find(".check-box.active")
        if (tds.find(".check-box").length === activeChecks.length)
            $("#roletask .tasks-holder th .check-box").addClass("active");
        else
            $("#roletask .tasks-holder th .check-box").removeClass("active");        
        e.preventDefault();
    });
    $(document).on("click", "#roletask .tasks-holder th .check-box", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        if (isActive) {
            $("#roletask .tasks-holder td .check-box").each(function (i, elm) {
                $(elm).addClass("active");
            });
        }
        else
            $("#roletask .tasks-holder td .check-box").removeClass("active");

    });

    $(document).on("click", "#roletask .tasks-holder td .check-box-s", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var tds = $("#roletask .tasks-holder td");
        var activeChecks = tds.find(".check-box-s.active")
        if (tds.find(".check-box-s").length === activeChecks.length)
            $("#roletask .tasks-holder th .check-box-s").addClass("active");
        else
            $("#roletask .tasks-holder th .check-box-s").removeClass("active");        
        e.preventDefault();
    });
    $(document).on("click", "#roletask .tasks-holder th .check-box-s", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        if (isActive) {
            $("#roletask .tasks-holder td .check-box-s").each(function (i, elm) {
                $(elm).addClass("active");
            });
        }
        else
            $("#roletask .tasks-holder td .check-box-s").removeClass("active");

    });

    $(document).on("click", "#roletask .tasks-holder td .check-box-r", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var tds = $("#roletask .tasks-holder td");
        var activeChecks = tds.find(".check-box-r.active")
        if (tds.find(".check-box-r").length === activeChecks.length)
            $("#roletask .tasks-holder th .check-box-r").addClass("active");
        else
            $("#roletask .tasks-holder th .check-box-r").removeClass("active");
        e.preventDefault();
    });
    $(document).on("click", "#roletask .tasks-holder th .check-box-r", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        if (isActive) {
            $("#roletask .tasks-holder td .check-box-r").each(function (i, elm) {
                $(elm).addClass("active");
            });
        }
        else
            $("#roletask .tasks-holder td .check-box-r").removeClass("active");

    });

    $(document).on("click", "#roletask .tasks-holder td .check-box-w", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var tds = $("#roletask .tasks-holder td");
        var activeChecks = tds.find(".check-box-w.active")
        if (tds.find(".check-box-w").length === activeChecks.length)
            $("#roletask .tasks-holder th .check-box-w").addClass("active");
        else
            $("#roletask .tasks-holder th .check-box-w").removeClass("active");
        e.preventDefault();
    });
    $(document).on("click", "#roletask .tasks-holder th .check-box-w", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        if (isActive) {
            $("#roletask .tasks-holder td .check-box-w").each(function (i, elm) {
                $(elm).addClass("active");
            });
        }
        else
            $("#roletask .tasks-holder td .check-box-w").removeClass("active");

    });

    $(document).on("click", "#tasks .close", function (e) {
        $("#tasks").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
            $(this).remove();
            $(".btm-overlay").fadeOut(200, function () { $(this).remove(); });
        });
    });

    // End role task detail


    //metarial constructor
    $(document).on("click", ".mat-const-main .const-holder ul li > .wo-actions .add-materials button", function (e) {
        console.log("asfasf")
        var self = $(this);
        self.toggleClass("off");
        if (self.classList.length > 0) {
            LoadMetarialUsagePopup(self);
        }
        e.preventDefault();
    });
    $(document).on("click", ".has-constructor .dt-details-mat", function (e) {
        var self = $(this);
        var wid = self.parents("tr").attr("id");
        wid = wid.toLowerCase().indexOf("invoiced") > -1 ? "invoiced" : wid.split("_")[1];
        if (wid != "invoiced") {
            loading(false);
            url = self.parents("table").data("maturl");
            $("body").append("<div class='full-overlay'></div>");
            $(".full-overlay").fadeIn(200);
            $.ajax({
                url: url,
                data: { wid: wid },
                type: "POST",
                success: function (html) {
                    removeLoader(false, 500, function () {
                        $("body").append(html);
                        $("#matConstructor [title]").tooltip({ placement: "top" });
                        $("#matConstructor").addClass("animated bounceInRight").show();
                        $('.mat-const-main .const-holder ul').perfectScrollbar();
                        $("#matConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                        $("#matConstructor .title span").dotdotdot();
                        var actHolder = $(".mat-const-main .const-holder.const-mats");
                        if (actHolder.find("li").length == 0) {
                            actHolder.find("ul").remove();
                            $(".mat-const-main .const-mats .placeholder").show(100, function () {
                                $(".mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                            });
                        }
                        else {
                            var ph = actHolder.find(".placeholder");
                            var time = ph.is(":visible") ? 300 : 10;
                            ph.find("button").removeClass("activated animated flash");
                            ph.hide(200);
                            setTimeout(function () {
                                setDisableMetarialOptions();
                                actHolder.find("select").select2({ width: "resolve" });
                                actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                                $('.mat-const-main .const-mats ul').perfectScrollbar();
                            }, time);
                        }
                    });
                }
            });
        }
        else {
            var htmlMsg = "<p>This Workorder has already been <b>Invoiced</b>. To edit the Workorder you must first reverse the Invoice.</p>";
            var htmlTitle = "<i class='fe-comment'></i>Already Processed";
            displayDialogMsg(htmlMsg, htmlTitle);
        }
        e.preventDefault();
    });
    
    //metarial constructor item actions
    $(document).on("click", "#matConstructor .const-mats > .title > a, #matConstructor .const-mats .placeholder button", function (e) {

        //if ($("#matConstructor .const-locs ul li.active").length == 0) {
        //    var msg = "<p>Please add and/or select a <b>Location</b> before attempting to add any new <b>Activities</b>.</p>";
        //    var title = "<i class='fe-comment'></i> Cannot Add Activity";
        //    displayDialogMsg(msg, title);
        //}
        //else {
            var matsHolder = $("#matConstructor > .mat-const-main .const-mats");
            var self = $(this);
            if (matsHolder.find("ul").length == 0)
                matsHolder.append("<ul></ul>");
            if (matsTempStorage == null) {
                $.ajax({
                    async: false,
                    url: '/workorder/getmetarials',
                    data: { actId: $("#actIdHidden").val() },
                    type: 'post',
                    success: function (r) {
                        matsTempStorage = r;
                    }
                })
            }
            var ph = $("#matConstructor .const-mats .placeholder");
            var time = ph.is(":visible") ? 300 : 10;
            ph.hide(200);
            setTimeout(function () {
                matsHolder.find("ul").append(applyActivityMatTemplate(matsTempStorage));
                $('.mat-const-main .const-mats ul').perfectScrollbar("update");
                setDisableMetarialOptions();
                matsHolder.find("select").select2({ width: "resolve", placeholder: "Select a Metarial" });
                matsHolder.find("input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                //matsHolder.find("input.act-amt").on("blur", function (e) {
                //    var self = $(this);
                //    var prnt = $(this.parentElement);
                //    var id = prnt.find("select").val();
                //    $.post("/workorder/GetMaxMetarialQty", { matId: id }, function (r) {
                //        var msg = "";
                //        var title = "<i class='fe-comment'></i>Quntity Warning!"
                //        if (r.MaxQty != 0) {
                //            if (Number(self.val()) > r.MaxQty)
                //                msg = "Quntity cannot be greater than " + r.MaxQty + ".";
                //        }
                //        if (Number(self.val()) < 1)
                //            msg = "Quntity cannot be 0 or minus value.";
                //        if (msg != "") {
                //            displayDialogMsg(msg, title);
                //            self.val(1.00);
                //        }
                //    });
                //});
            }, time);
        //}
        e.preventDefault();
    });
    $(document).on("change", "#matConstructor .const-mats select", function () {
        var self = $(this);
        var id = self.val();
        setDisableActivityOptions();
        for (var i = 0; i < matsTempStorage.length; i++) {
            if (matsTempStorage[i].activityID == id) {
                var elm = self.parents("li").find(".act-amt");
                $(elm[0]).val(matsTempStorage[i].maxQty);
            }
        }
        //$.get("/workorder/hasmaterial", { id: id }, function (result) {
        //    var parent = self.parents("li").find(".wo-actions");
        //    if (result == true) {
        //        if (parent.find(".mat-action").length < 1) {
        //            parent.append('<a href="#" class="mat-action"><i class="mdi mdi-server"></i></a>');// style="color: green;"
        //        }
        //    }
        //    else {
        //        parent.find(".mat-action").remove();
        //    }
        //});
    });

    $(document).on("blur", "#matConstructor .const-mats input.act-amt", function (e) {
        var self = $(this);
        var prnt = $(this.parentElement);
        var id = prnt.find("select").val();
        var actId = $("#actIdHidden").val();
        $.post("/workorder/GetMaterialMaxQty", { actId: actId, matId: id }, function (r) {
            var msgAddl = "";
            if (r.MaxQty != 0) {
                if (Number(self.val()) > r.MaxQty)
                    msgAddl = "You have exceed the <b class='default-col'>Maximum Quantity ( " + r.MaxQty + " )</b> level for the material.";
            }
            if (Number(self.val()) < 1)
                msgAddl = "You are going to insert <b class='default-col'>Nagavite</b> value as material quantity.";
            if (msgAddl != "") {
                var buttons = [
                        { text: "Continue", click: function () { $(this).dialog("close"); } },
                        { text: "Cancel", click: function () { $(this).dialog("close"); } }
                ];
                var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
                displayConfirmationMsg(msg, 'Quantity Warning', buttons);
            }
        });
    });

    //metarial constructor top btns
    $(document).on("click", "#matConstructor > .title .btn-group label:not(.active)", function (e) {
        var self = $(this);
        var selfCls = self.attr("class");
        var type = selfCls.indexOf("unsubmit") > -1 ? "unsubmit" : selfCls.indexOf("verify") > -1 ? "verify" : "submit";
        $.post("/workorder/changeStatus", { type: type, wid: $("#constructor").data("id") }, function (r) {
            if (r.Msg != "") {
                $("#constructor > .title .btn-group label").removeClass("active");
                if (type == "submit")
                    $("#constructor > .title .btn-group label.submit").addClass("active");
                else if (type == "verify")
                    $("#constructor > .title .btn-group label.verify").addClass("active");
                else
                    $("#constructor > .title .btn-group label.unsubmit").addClass("active");
                var msg = r.Msg;
                var title = "<i class='fe-comment'></i> Cannot Change Status!"
                if (r.Code == "1001")
                    title = "<i class='fe-comment'></i>Penalty Warning!"
                displayDialogMsg(msg, title);
            }
        });
    });

    //metarial constructor edit and delete actions    
    $(document).on("click", ".mat-const-main .const-holder ul li .del-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        if (temp.attr("class").indexOf("new") > -1) {
            temp.remove();
        }
        else {
            var holder = self.parents(".const-holder");
            var type = "Activity";
            var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); deleteMetarial(type); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msgAddl = "This action will <b class='default-col'>Delete</b> this <b class='act-col'>Metarial</b>."
            var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Delete ' + type, buttons);
        }
        e.stopPropagation();
        e.preventDefault();
    });
    //metarial constructor bottom btns

    //$(document).on("click", "#matConstructor .process", function (e) {
    //    var id = $("#matConstructor").data("id");
    //    loading(true);
    //    $.get("../workorder/workorderactivitycount", { id: id }, function (data) {
    //        if (!data.any || !data.verified) {
    //            var title = "<i class='fe-comment'></i> Cannot Process Work Order!"
    //            var msg = "";
    //            removeLoader(true, 500, function () {
    //                if (!data.any)
    //                    msg = "<p>Cannot <b class='default-col'>Process</b> a Work Order that has no activities. Please make sure that at least one (1) <b class='act-col'>Activity</b> was created.</p>";
    //                else
    //                    msg = "<p>Cannot <b class='default-col'>Process</b> a Work Order that has not yet been <b class='act-col'>Verified</b>. Please make sure that you have changed the Work Order's status.</p>";
    //                displayDialogMsg(msg, title);
    //            });
    //        }
    //        else {
    //            var invID = $("#constructor .wo-inv select").val();
    //            var title = $("#constructor .title-editor input").val();
    //            var buttons = [
    //                        {
    //                            text: "Continue", click: function () {
    //                                $(this).dialog("close");
    //                                loading(true);
    //                                $.post("../workorder/processactivities", { id: id, invID: invID, title: title }, function (data) {
    //                                    removeLoader(true, 500, function () {
    //                                        if (typeof data.Additional === 'undefined') {
    //                                            $("#constructor").remove();
    //                                            $("body").append(data);
    //                                            $("#constructor").show();
    //                                        }
    //                                        msg = "<p>Successfully <b class='default-col'>Processed</b> Workorder. </p>";
    //                                        var title = "<i class='fe-comment'></i> Success!"
    //                                        displayDialogMsg(msg, title);
    //                                    });
    //                                });
    //                            }
    //                        },
    //                        { text: "Cancel", click: function () { $(this).dialog("close"); } }
    //            ];
    //            var msg = '<p>Are you sure you want to <b class="default-col">Process</b> this Work Order?</p></div>';
    //            removeLoader(true, 500, function () {
    //                displayConfirmationMsg(msg, 'Process Work Order', buttons);
    //            });
    //        }
    //    });
    //    e.preventDefault();
    //});

    $(document).on("click", "#matConstructor .cancel", function (e) {
        var stop = discardMetarialDataCheck("full");
        if (stop) {
            if (!isMatAddedToist) {
                var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); closeMatConstructor(); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
                ];
                var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
                displayConfirmationMsg(msg, 'Discard Changes', buttons);
            }
            else
                closeMatConstructor();
        }
        else
            closeMatConstructor();
        e.preventDefault();
    });
    $(document).on("click", "#matConstructor .update", function (e) {
        var mats = [];
        $(".mat-const-main .const-holder.const-mats ul li").each(function () {
            var self = $(this);
            var actID = $("#matConstructor #actIdHidden").val();
            var loc = $("#matConstructor #locHidden").val();
            var date = $("#matConstructor #dateHidden").val();
            var matID = self.find("select").val();
            var matQty = self.find(".act-amt").val();
            if (!matID.isEmpty() && !matQty.isEmpty()) {
                mats.push({
                    actID: actID,
                    loc: loc,
                    date: date,
                    matID: matID,
                    matQty: matQty
                });
            }
        });
        
        if (mats.length > 0) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); updateMetarial(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = "<p>Only the valid <b class='act-col'>Metarials</b> will be <b class='default-col'>Added to the list </b> metarials will be saved, when <b class='default-col'>saving the activities</b>. Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Update Confirmation', buttons);
        }
        else {
            var msg = "<p>Cannot <b class='default-col'>Update</b> incomplete Work order. Please make sure that at least one (1) <b class='act-col'>Metarial</b> was selected with an appropriate quantity.</p>";
            var title = "<i class='fe-comment'></i> Cannot Update Work Order!"
            displayDialogMsg(msg, title);
        }
        e.preventDefault()
    });
    // End metarial constructor

    //quotation constructor

    //$(document).on("click", ".mat-const-main .const-holder ul li > .wo-actions .add-materials button", function (e) {
    //    console.log("asfasf")
    //    var self = $(this);
    //    self.toggleClass("off");
    //    if (self.classList.length > 0) {
    //        LoadMetarialUsagePopup(self);
    //    }
    //    e.preventDefault();
    //});

    $(document).on("click", ".has-constructor .dt-details-quot", function (e) {
        var self = $(this);
        var qid = self.parents("tr").attr("id");

        loading(false);
        url = self.parents("table").data("qurl");
        $("body").append("<div class='full-overlay'></div>");
        $(".full-overlay").fadeIn(200);
        $.ajax({
            url: url,
            data: { qid: qid },
            type: "POST",
            success: function (html) {
                removeLoader(false, 500, function () {
                    $("body").append(html);
                    $("#qConstructor [title]").tooltip({ placement: "top" });
                    $("#qConstructor").addClass("animated bounceInRight").show();
                    $('#qConstructor .mat-const-main .const-holder ul').perfectScrollbar();
                    $("#qConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                    $("#qConstructor .title span").dotdotdot();
                    var actHolder = $("#qConstructor .mat-const-main .const-holder.const-mats");
                    if (actHolder.find("li").length == 0) {
                        actHolder.find("ul").remove();
                        $("#qConstructor .mat-const-main .const-mats .placeholder").show(100, function () {
                            $("#qConstructor .mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                        });
                    }
                    else {
                        var ph = actHolder.find(".placeholder");
                        var time = ph.is(":visible") ? 300 : 10;
                        ph.find("button").removeClass("activated animated flash");
                        ph.hide(200);
                        setTimeout(function () {
                            setDisableQuotationActivityOptions();
                            actHolder.find("select").select2({ width: "resolve" });
                            actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                            $('#qConstructor .mat-const-main .const-mats ul').perfectScrollbar();
                        }, time);
                    }
                });
            }
        });
        e.preventDefault();
    });
    
    //quotation constructor item actions
    $(document).on("click", "#qConstructor .const-mats > .title > a, #qConstructor .const-mats .placeholder button", function (e) {
        var matsHolder = $("#qConstructor > .mat-const-main .const-mats");
        var self = $(this);
        if (matsHolder.find("ul").length == 0)
            matsHolder.append("<ul></ul>");
        if (actsTempStorage == null) {
            $.ajax({
                async: false,
                url: '/quotation/GetActivities',
                type: 'post',
                success: function (r) {
                    actsTempStorage = r;
                }
            })
        }
        var ph = $("#qConstructor .const-mats .placeholder");
        var time = ph.is(":visible") ? 300 : 10;
        ph.hide(200);
        setTimeout(function () {
            matsHolder.find("ul").append(applyQuotActivityTemplate(actsTempStorage));
            $('#qConstructor .mat-const-main .const-mats ul').perfectScrollbar("update");
            setDisableQuotationActivityOptions();
            matsHolder.find("select").select2({ width: "resolve", placeholder: "Select a Activity", tags: true });
            matsHolder.find("input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
            matsHolder.find("input.act-amt").on("blur", function (e) {
                var self = $(this);
                var prnt = $(this.parentElement);
                var val = prnt.find("select").val();
                $.post("/quotation/GetMaxQty", { act: val }, function (r) {
                    var msg = "";
                    var title = "<i class='fe-comment'></i>Quntity Warning!"
                    if (r.MaxQty != 0) {
                        if (Number(self.val()) > r.MaxQty)
                            msg = "Quntity cannot be greater than " + r.MaxQty + ".";                        
                    }
                    if (Number(self.val()) < 1)
                        msg = "Quntity cannot be 0 or minus value.";
                    if (msg != "") {
                        displayDialogMsg(msg, title);
                        self.val(1.00);
                    }
                    self.parent("li").find("input.act-rat").val(r.Rate);
                });
            });
        }, time);
        e.preventDefault();
    });
    $(document).on("change", "#qConstructor .const-mats select", function () {
        var self = $(this);
        var val = self.val();
        $.post("/quotation/GetRate", { act: val }, function (r) {
            var prnt = self.parent(".act-holder");
            var elm = prnt.find(".act-rat");
            elm.val(r.Rate);
        });
        setDisableQuotationActivityOptions();
    });
        
    //metarial constructor edit and delete actions    
    $(document).on("click", "#qConstructor .mat-const-main .const-holder ul li .del-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        if (temp.attr("class").indexOf("new") > -1) {
            temp.remove();
        }
        else {
            var holder = self.parents(".const-holder");
            var type = "Activity";
            var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); deleteQuotationActivity(type); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msgAddl = "This action will <b class='default-col'>Delete</b> this <b class='act-col'>Activity</b>."
            var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Delete ' + type, buttons);
        }
        e.stopPropagation();
        e.preventDefault();
    });

    //quotation constructor bottom btns
    $(document).on("click", "#qConstructor .cancel", function (e) {
        var stop = discardQuotationDataCheck("full");
        if (stop) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); closeQuotConstructor(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
            displayConfirmationMsg(msg, 'Discard Changes', buttons);
        }
        else
            closeQuotConstructor();
        e.preventDefault();
    });
    $(document).on("click", "#qConstructor .update", function (e) {
        var mats = [];
        $("#qConstructor .mat-const-main .const-holder.const-mats ul li").each(function () {
            var self = $(this);
            var matID = self.find("select").val();
            var matQty = self.find(".act-amt").val();
            if (!matID.isEmpty() && !matQty.isEmpty()) {
                mats.push({
                    matID: matID,
                    matQty: matQty
                });
            }
        });

        if (mats.length > 0) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); updateQuotationActivity(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = "<p>Only the valid <b class='act-col'>Activities</b> will be saved. Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Update Confirmation', buttons);
        }
        else {
            var msg = "<p>Cannot <b class='default-col'>Update</b> incomplete Quotation. Please make sure that at least one (1) <b class='act-col'>Activity</b> was selected with an appropriate quantity.</p>";
            var title = "<i class='fe-comment'></i> Cannot Update Quotation!"
            displayDialogMsg(msg, title);
        }
        e.preventDefault()
    });

    // End quotation constructor


    //Standby Invoice Constructor
    $(document).on("click", ".has-constructor .dt-details-sinv", function (e) {
        var self = $(this);
        var sIid = self.parents("tr").attr("id");

        loading(false);
        url = self.parents("table").data("qurl");
        $("body").append("<div class='full-overlay'></div>");
        $(".full-overlay").fadeIn(200);
        $.ajax({
            url: url,
            data: { sIid: sIid },
            type: "POST",
            success: function (html) {
                removeLoader(false, 500, function () {
                    $("body").append(html);
                    $("#sIConstructor [title]").tooltip({ placement: "top" });
                    $("#sIConstructor").addClass("animated bounceInRight").show();
                    $('#sIConstructor .mat-const-main .const-holder ul').perfectScrollbar();
                    $("#sIConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                    $("#sIConstructor .title span").dotdotdot();
                    var actHolder = $("#sIConstructor .mat-const-main .const-holder.const-mats");
                    if (actHolder.find("li").length == 0) {
                        actHolder.find("ul").remove();
                        $("#sIConstructor .mat-const-main .const-mats .placeholder").show(100, function () {
                            $("#sIConstructor .mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                        });
                    }
                    else {
                        var ph = actHolder.find(".placeholder");
                        var time = ph.is(":visible") ? 300 : 10;
                        ph.find("button").removeClass("activated animated flash");
                        ph.hide(200);
                        setTimeout(function () {
                            setDisableSInvoiceActivityOptions();
                            actHolder.find("select").select2({ width: "resolve" });
                            actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                            $('#sIConstructor .mat-const-main .const-mats ul').perfectScrollbar();
                        }, time);
                    }
                });
            }
        });
        e.preventDefault();
    });

    //Standby Invoice constructor item actions
    $(document).on("click", "#sIConstructor .const-mats > .title > a, #sIConstructor .const-mats .placeholder button", function (e) {
        var matsHolder = $("#sIConstructor > .mat-const-main .const-mats");
        var self = $(this);
        if (matsHolder.find("ul").length == 0)
            matsHolder.append("<ul></ul>");
        if (actsTempStorage == null) {
            $.ajax({
                async: false,
                url: '/StandbyInvoice/GetActivities',
                type: 'post',
                success: function (r) {
                    actsTempStorage = r;
                }
            })
        }
        var ph = $("#sIConstructor .const-mats .placeholder");
        var time = ph.is(":visible") ? 300 : 10;
        ph.hide(200);
        setTimeout(function () {
            matsHolder.find("ul").append(applySInvActivityTemplate(actsTempStorage));
            $('#sIConstructor .mat-const-main .const-mats ul').perfectScrollbar("update");
            setDisableSInvoiceActivityOptions();
            matsHolder.find("select").select2({ width: "resolve", placeholder: "Select a Activity", tags: true });
            matsHolder.find("input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
            matsHolder.find("input.act-amt").on("blur", function (e) {
                var self = $(this);
                var prnt = $(this.parentElement);
                var val = prnt.find("select").val();
                $.post("/StandbyInvoice/GetMaxQty", { act: val }, function (r) {
                    var msg = "";
                    var title = "<i class='fe-comment'></i>Quntity Warning!"
                    if (r.MaxQty != 0) {
                        if (Number(self.val()) > r.MaxQty)
                            msg = "Quntity cannot be greater than " + r.MaxQty + ".";
                    }
                    if (Number(self.val()) < 1)
                        msg = "Quntity cannot be 0 or minus value.";
                    if (msg != "") {
                        displayDialogMsg(msg, title);
                        self.val(1.00);
                    }
                    self.parent("li").find("input.act-rat").val(r.Rate);
                });
            });
        }, time);
        e.preventDefault();
    });
    $(document).on("change", "#sIConstructor .const-mats select", function () {
        var self = $(this);
        var val = self.val();
        $.post("/StandbyInvoice/GetRate", { act: val }, function (r) {
            var prnt = self.parent(".act-holder");
            var elm = prnt.find(".act-rat");
            elm.val(r.Rate);
        });
        setDisableSInvoiceActivityOptions();
    });
    $(document).on("click", "#sIConstructor .mat-const-main .const-holder ul li .del-action", function (e) {
        var self = $(this);
        temp = self.parents("li");
        if (temp.attr("class").indexOf("new") > -1) {
            temp.remove();
        }
        else {
            var holder = self.parents(".const-holder");
            var type = "Activity";
            var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); deleteSInvoiceActivity(type); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msgAddl = "This action will <b class='default-col'>Delete</b> this <b class='act-col'>Activity</b>."
            var msg = "<div><p>" + msgAddl + " Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Delete ' + type, buttons);
        }
        e.stopPropagation();
        e.preventDefault();
    });


    //Standby Invoice constructor bottom btns
    $(document).on("click", "#sIConstructor .cancel", function (e) {
        var stop = discardSInvoiceDataCheck("full");
        if (stop) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); closeSInvoiceConstructor(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = '<p>Are you sure you want to continue this action. Any unsaved changes will be <b>Discarded</b>!</p></div>';
            displayConfirmationMsg(msg, 'Discard Changes', buttons);
        }
        else
            closeSInvoiceConstructor();
        e.preventDefault();
    });
    $(document).on("click", "#sIConstructor .update", function (e) {
        var mats = [];
        $("#sIConstructor .mat-const-main .const-holder.const-mats ul li").each(function () {
            var self = $(this);
            var matID = self.find("select").val();
            var matQty = self.find(".act-amt").val();
            if (!matID.isEmpty() && !matQty.isEmpty()) {
                mats.push({
                    matID: matID,
                    matQty: matQty
                });
            }
        });

        if (mats.length > 0) {
            var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); updateSInvoiceActivity(); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
            ];
            var msg = "<p>Only the valid <b class='act-col'>Activities</b> will be saved. Are you sure you want to Continue?</p></div>";
            displayConfirmationMsg(msg, 'Update Confirmation', buttons);
        }
        else {
            var msg = "<p>Cannot <b class='default-col'>Update</b> incomplete Standby Invoice. Please make sure that at least one (1) <b class='act-col'>Activity</b> was selected with an appropriate quantity.</p>";
            var title = "<i class='fe-comment'></i> Cannot Update Standby Invoice!"
            displayDialogMsg(msg, title);
        }
        e.preventDefault()
    });

    // End Standby Invoice constructor

    
    $(document).on('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', ".tel-form .msg-bar", function () {
        $(this).removeClass("animated wobble pulse");
    });
    $(document).on("click", ".tel-form .proportion", function (e) {
        $(this).toggleClass("set unset").find("i").toggleClass("fe-infinity-1 fe-waves");
        $(".tel-form .splits").first().trigger("keyup");
        e.preventDefault();
    });
    $(document).on("keyup", ".tel-form .splits", function () {
        if ($(".tel-form .proportion").is(".set")) {
            var self = $(this);
            var otherSplit = self.parents("tr").find(".splits").not(self);
            var splitVal = self.val();
            otherSplit.val(splitVal > 100 ? 0 : 100 - self.val());
        }
    });
    $(document).on("change", '.team-setup .con-ddl', function () {
        var self = $(this);
        var url = self.select2("data")[0].imgUrl;
        var payScal = self.select2("data")[0].payScale;
        var payScaleId = self.select2("data")[0].payScaleId;
        var portion = self.select2("data")[0].Portion;
        var plateNo = self.select2("data")[0].PlateNo;
        var id = self.select2("data")[0].id;
        var tr = self.parents("tr");
        tr.find("img").attr("src", url);
        tr.find("label").text(payScal);
        tr.find("#contractorId").val(id);
        if (tr.find("#memberPayScale") != undefined)
            tr.find("#memberPayScale").val(payScaleId);
        if (document.getElementById("contractorPortion") != undefined)
            $("#contractorPortion").val(portion != undefined && portion != "" ? portion : 100);
        if (plateNo != undefined && plateNo != "") {
            if (document.getElementById("Own1") != undefined)
                $("#Own1").prop("checked", true)
            var plateNoSelect = document.getElementById("plateNoDrop");
            var option = document.createElement("option");
            option.text = plateNo + " - Own Vehicale";
            option.value = plateNo;
            if (plateNoSelect != undefined && plateNoSelect != null)
                plateNoSelect.add(option, 0);
            $("#plateNoDrop").val(plateNo);
            if (tr.find("#memberPlateNo") != undefined)
                tr.find("#memberPlateNo").val(plateNo);
        }
        if (document.getElementById("techNoLbl") != undefined) {

        }
        tr.data("id", self.val());
    });
    $(document).on("click", ".tel-form .team-setup .add-new-mem", function (e) {
        if (($(".tel-form .team-setup tr").length - 1) < parseInt($(".tel-form .team-setup").data("team"))) {
            $.get("workorder/getcontractorpartial", function (html) {
                $(".tel-form .team-setup").append(html);
                setupTeamActions(false);
            });
        }
        e.preventDefault();
    });
    $(document).on("click", ".tel-form .team-setup .remove-mem", function (e) {
        $(this).parents("tr").remove();
        e.preventDefault();
    });
    $(document).on("click", ".team-btn", function (e) {
        SubmitTeam(false);
        e.stopPropagation();
        e.preventDefault();
    });
    function SubmitTeam(fromDetail) {
        var team = [];
        var ok = false;
        $(".tel-form .team-setup tr.team-mem").each(function () {
            var tr = $(this);
            var id = tr.data("id").toString();
            var share = tr.find(".portion").val();
            var isLead = tr.find(".lead").val();
            if (!id.isEmpty() && !share.isEmpty()) {
                team.push({
                    id: id,
                    share: share,
                    islead: isLead.toLowerCase() == "false" ? false : true
                });
                ok = true;
            }
            else {
                ok = false;
                return false;
            }
        });
        var anyleader = team.filter(function (elm) {
            return elm.islead == true;
        });
        if (ok && anyleader.length > 0) {
            var data = {
                wid: $(".tel-form-id").val(),
                team: team
            };
            $.post("workorder/saveteam", $.toDictionary({ model: data }), function (data) {
                if (data.Code != "100")
                    setupMsgBar(3, "Create/Update of Team failed. Please try again or contact your system administrator");
                else {
                    setupMsgBar(2, "Team was Saved/Update successfully!");
                    var page = dTable.page();
                    dTable.page(page).draw(false);
                    //dTable.draw();
                    if (!fromDetail) {
                        SubmitWO(self, true);
                    }
                    setTimeout('$(".close").click()', 2000);
                }
            });
        }
        else
            setupMsgBar(3, "Saving team failed. Please ensure that all inputs were entered and that a leader was selected.");        
    }
    //function SaveWO() {
    //    var form = $(".tel-form form");

    //    var team = [];
    //    var ok = false;
    //    $(".tel-form .team-setup tr.team-mem").each(function () {
    //        var tr = $(this);
    //        var id = tr.data("id").toString();
    //        var share = tr.find(".portion").val();
    //        var isLead = tr.find(".lead").val();
    //        if (!id.isEmpty() && !share.isEmpty()) {
    //            team.push({
    //                id: id,
    //                share: share,
    //                islead: isLead.toLowerCase() == "false" ? false : true
    //            });
    //            ok = true;
    //        }
    //        else {
    //            ok = false;
    //            return false;
    //        }
    //    });
    //    if (team.length > 0) {
    //        var anyleader = team.filter(function (elm) {
    //            return elm.islead == true;
    //        });
    //        if (ok && anyleader.length > 0) {
    //            var data = {
    //                wid: $(".tel-form-id").val(),
    //                team: team
    //            };
    //            $.post("workorder/saveteam", $.toDictionary({ model: data }), function (data) {
    //                if (data.Code != "100")
    //                    setupMsgBar(3, "Create/Update of Team failed. Please try again or contact your system administrator");
    //                else
    //                    setupMsgBar(2, "Team was Saved/Update successfully!");
    //            });
    //        }
    //        else
    //            setupMsgBar(3, "Saving team failed. Please ensure that all inputs were entered and that a leader was selected.");
    //        e.stopPropagation();
    //    }
    //}
    $(document).on("click", ".merged-singular ", function (e) {
        var self = $(this);
        self.toggleClass("merged");
        $(".merge-adder").toggle(200);
        self.find("i").toggleClass("fe-shuffle fe-list")
        $(".slider.active").hide("slide", { direction: "right" }, 200, function () {
            var $this = $(this);
            $(".slider").not($this).show("slide", { direction: "left" }, 200).addClass("active")
        }).removeClass("active");
    });
    $(document).on("click", ".merge-adder", function (e) {
        loading(false);
        $.get("workorder/Merger", function (html) {
            $("body").append("<div class='btm-overlay'></div>");
            $(".btm-overlay").fadeIn(200);
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#merger-left ul, #merger-right .center").perfectScrollbar();
            });
        });
    });
    $(document).on("click", ".dt-edit-merge", function (e) {
        var self = $(this);
        var row = self.parents("tr");
        var id = $(this).parents("tr").attr("id");
        id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
        loading(false);
        $.get("workorder/GetMerger", { id: id }, function (html) {
            $("body").append("<div class='btm-overlay'></div>");
            $(".btm-overlay").fadeIn(200);
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#merger-left ul, #merger-right .center").perfectScrollbar();
            });
        });
        e.preventDefault();
    });
    $(document).on("click", ".dt-delete-merge", function (e) {
        var self = $(this);
        var itemName = self.parents("tr").find(".dt-item-refnum").text();
        var itemType = self.parents("table").data("name");
        var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); deleteMergeRow(self); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
        ];
        var msg = '<p>Are you sure you want to <b class="default-col">Delete</b> <b>' + itemName + '</b> record?</p></div>';
        displayConfirmationMsg(msg, 'Delete ' + itemType, buttons);
        e.preventDefault();
    });
    $(document).on("keyup", "#merger-left .filter input", function () {
        var val = $(this).val();
        var holder = $("#merger-left ul");
        $("#merger-left ul").perfectScrollbar('destroy');
        $.get("workorder/GetMergeReferences", { val: val }, function (data) {
            holder.html(data);
            $("#merger-left ul").perfectScrollbar();
        });
    });
    $(document).on("click", "#merger-left ul li:not(.empty)", function () {        
        var self = $(this);
        if ($("#mergerIdHidden").val() != "" && $("#mergerIdHidden").val() != undefined) {
            LoadMergedGridWithMerged(self, $("#mergerIdHidden").val());
            return;
        }
        $("#merger-left ul li:not(.empty)").removeClass("active");
        var val = self.data("code");
        self.addClass("active");
        var holder = $("#merger-right .workorder-holder");
        $("#merger-right .center").perfectScrollbar('destroy');
        $.get("workorder/MergeCandiditates", { code: val }, function (data) {
            holder.html(data);
            $("#merger-right .total.tot").data("total", 0).find("span").text("$0.00");
            $("#merger-right .center").perfectScrollbar();
            CalculateTotal();
        });
    });
    $(document).on("click", "#merger-right .workorder-holder td .check-box", function (e) {
        var self = $(this);
        var totHolder = $("#merger-right .total.tot");
        totHolder.removeClass("pulse");
        self.toggleClass("active");
        var total = parseFloat(self.parents("tr").find("td:last-child").data("total"));
        total *= self.is(".active") ? 1 : -1;
        var oldTotal = parseFloat(totHolder.data("total"));
        var newTotal = total + oldTotal;
        totHolder.find("span").text(newTotal.formatMoney(2, "$"));
        totHolder.data("total", newTotal).addClass("pulse");
        var tds = $("#merger-right .workorder-holder td");
        var activeChecks = tds.find(".check-box.active")
        if (tds.find(".check-box").length === activeChecks.length)
            $("#merger-right .workorder-holder th .check-box").addClass("active");
        else
            $("#merger-right .workorder-holder th .check-box").removeClass("active");
        if (activeChecks.length > 1)
            $("#merger-right .merger-btn").show(200);
        else
            $("#merger-right .merger-btn").hide(200);
        e.preventDefault();
    });
    $(document).on("click", "#merger-right .workorder-holder th .check-box", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        var total = 0;
        if (isActive) {
            $("#merger-right .workorder-holder td .check-box").each(function (i, elm) {
                $(elm).addClass("active");
                total += parseFloat($(elm).parents("tr").find("td:last-child").data("total"));
            });
        }
        else
            $("#merger-right .workorder-holder td .check-box").removeClass("active");

        var totHolder = $("#merger-right .total.tot");
        totHolder.removeClass("pulse");
        totHolder.find("span").text(total.formatMoney(2, "$"));
        totHolder.data("total", total).addClass("pulse");
        if (isActive)
            $("#merger-right .merger-btn").show(200);
        else
            $("#merger-right .merger-btn").hide(200);
    });
    $(document).on("click", "#merger .close", function (e) {
        $("#merger").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
            $(this).remove();
            $(".btm-overlay").fadeOut(200, function () { $(this).remove(); });
        });
    });
    $(document).on("click", "#merger .merger-process", function (e) {
        var code = $("#merger-left ul li.active").data("code");
        if (code != undefined && code != "") {
            loading(true);
            $.get("../workorder/mergeractivitycount", { id: $("#mergerIdHidden").val() }, function (data) {
                if (!data.any || !data.verified) {
                    var title = "<i class='fe-comment'></i> Cannot Process!"
                    var msg = "";
                    removeLoader(true, 500, function () {
                        if (data.msg == "") {
                            if (!data.any)
                                msg = "<p>Cannot <b class='default-col'>Process</b> the Merged Work Orders those have no activities. Please make sure that at least one (1) <b class='act-col'>Activity</b> was created.</p>";
                            else
                                msg = "<p>Cannot <b class='default-col'>Process</b> the Merged Work Orders those have not yet been <b class='act-col'>Verified</b>. Please make sure that you have changed the Work Order's status.</p>";
                        }
                        else
                            msg = "<p>Cannot <b class='default-col'>Process</b>. Please make sure that you have <b class='default-col'>Merged</b> the Work Orders.</p>";
                        displayDialogMsg(msg, title);
                    });
                }
                else {
                    var invID = $("#merger select").val();
                    var buttons = [
                                {
                                    text: "Continue", click: function () {
                                        $(this).dialog("close");
                                        loading(true);
                                        $.post("../workorder/processmerger", { code: code, invID: invID, id: $("#mergerIdHidden").val(), title: $("#txtTitle").val() }, function (data) {
                                            removeLoader(true, 500, function () {
                                                if (typeof data.Additional === 'undefined') {
                                                    $("#merger").remove();
                                                    $("body").append(data);
                                                    $("#merger").show();
                                                }
                                                msg = "<p>Successfully <b class='default-col'>Processed</b> Work Orders. </p>";
                                                var title = "<i class='fe-comment'></i> Success!"
                                                displayDialogMsg(msg, title);
                                            });
                                        });
                                    }
                                },
                                { text: "Cancel", click: function () { $(this).dialog("close"); } }
                    ];
                    var msg = '<p>Are you sure you want to <b class="default-col">Process</b> this Merged Work Orders?</p></div>';
                    removeLoader(true, 500, function () {
                        displayConfirmationMsg(msg, 'Process Merged Work Orders', buttons);
                    });
                }
            });
        }
        else {
            var title = "<i class='fe-comment'></i> Cannot Process!"
            var msg = "<p>Cannot <b class='default-col'>Process</b>. Please make sure that you have selected the <b class='default-col'>Merged</b> Work Orders.</p>";
            displayDialogMsg(msg, title);
        }
        e.preventDefault();
    });

    /*New invoices*/

    $(document).on("click", ".inv-singular ", function (e) {
        var self = $(this);
        self.toggleClass("old-invoice");
        $(".inv-adder").toggle(200);
        self.find("i").toggleClass("fe-shuffle fe-list")
        $(".slider.active").hide("slide", { direction: "right" }, 200, function () {
            var $this = $(this);
            $(".slider").not($this).show("slide", { direction: "left" }, 200).addClass("active")
        }).removeClass("active");
    });
    $(document).on("click", ".inv-adder", function (e) {
        loading(false);
        $.get("invoice/newinv", function (html) {
            $("body").append("<div class='btm-overlay'></div>");
            $(".btm-overlay").fadeIn(200);
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#inv-left ul, #inv-right .center").perfectScrollbar();
            });
        });
    });
    $(document).on("click", ".dt-edit-merge", function (e) {
        var self = $(this);
        var row = self.parents("tr");
        var id = $(this).parents("tr").attr("id");
        id = id.indexOf("_") > -1 ? id.split("_")[1] : id;
        loading(false);
        $.get("invoice/GetNewInvoiceCreator", { id: id }, function (html) {
            $("body").append("<div class='btm-overlay'></div>");
            $(".btm-overlay").fadeIn(200);
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#inv-left ul, #inv-right .center").perfectScrollbar();
            });
        });
        e.preventDefault();
    });
    $(document).on("click", ".dt-delete-inv", function (e) {
        var self = $(this);
        var itemName = self.parents("tr").find(".dt-item-refnum").text();
        var itemType = self.parents("table").data("name");
        var buttons = [
                { text: "Continue", click: function () { $(this).dialog("close"); deleteInvRow(self); } },
                { text: "Cancel", click: function () { $(this).dialog("close"); } }
        ];
        var msg = '<p>Are you sure you want to <b class="default-col">Delete</b> <b>' + itemName + '</b> record?</p></div>';
        displayConfirmationMsg(msg, 'Delete ' + itemType, buttons);
        e.preventDefault();
    });
    //$(document).on("click", "#inv-left .inv-bot button", function () {
    //    debugger;
    //    var val = $("#inv-left .filter input").val();
    //    var holder = $("#inv-left ul");
    //    $("#inv-left ul").perfectScrollbar('destroy');
    //    $.get("invoice/GetMergeReferences", { val: val }, function (data) {
    //        holder.html(data);
    //        $("#inv-left ul").perfectScrollbar();
    //    });
    //});
    $(document).on("click", "#inv-left .inv-bot button", function () {
        var form = $("#inv-left form");
        var dateFrom = $("#inv-left [name='dateFrom']").val();
        var dateTo = $("#inv-left [name='dateTo']").val();
        //var self = $(this);
        var formdata = {
            client: $("#client").val(),
            po: $("#po").val(),
            classifi: $("#classifi").val(),
            region: $("#region").val(),
            dateFrom: dateFrom,
            dateTo: dateTo
        };
        if ($("#invIdHidden").val() != "" && $("#invIdHidden").val() != undefined) {
            LoadInvGridWithInv(formdata, $("#invIdHidden").val());
            return;
        }
        //$("#inv-left ul li:not(.empty)").removeClass("active");
        //var val = self.data("code");
        //self.addClass("active");
        var holder = $("#inv-right .workorder-holder");
        $("#inv-right .center").perfectScrollbar('destroy');
        $.post("invoice/InvoiceCandiditates", { model: formdata }, function (data) {
            holder.html(data);
            $("#inv-right .total.tot").data("total", 0).find("span").text("$0.00");
            $("#inv-right .center").perfectScrollbar();
            CalculateWOTotal();
        });
    });
    $(document).on("click", "#inv-left ul li:not(.empty)", function () {
        var self = $(this);
        if ($("#invIdHidden").val() != "" && $("#invIdHidden").val() != undefined) {
            LoadInvGridWithInv(self, $("#invIdHidden").val());
            return;
        }
        $("#inv-left ul li:not(.empty)").removeClass("active");
        var val = self.data("code");
        self.addClass("active");
        var holder = $("#inv-right .workorder-holder");
        $("#inv-right .center").perfectScrollbar('destroy');
        $.get("invoice/MergeCandiditates", { code: val }, function (data) {
            holder.html(data);
            $("#inv-right .total.tot").data("total", 0).find("span").text("$0.00");
            $("#inv-right .center").perfectScrollbar();
            CalculateWOTotal();
        });
    });
    $(document).on("click", "#inv-right .workorder-holder td .check-box", function (e) {
        var self = $(this);
        var totHolder = $("#inv-right .total.tot");
        totHolder.removeClass("pulse");
        self.toggleClass("active");
        var total = parseFloat(self.parents("tr").find("td:last-child").data("total"));
        total *= self.is(".active") ? 1 : -1;
        var oldTotal = parseFloat(totHolder.data("total"));
        var newTotal = total + oldTotal;
        totHolder.find("span").text(newTotal.formatMoney(2, "$"));
        totHolder.data("total", newTotal).addClass("pulse");
        var tds = $("#inv-right .workorder-holder td");
        var activeChecks = tds.find(".check-box.active")
        if (tds.find(".check-box").length === activeChecks.length)
            $("#inv-right .workorder-holder th .check-box").addClass("active");
        else
            $("#inv-right .workorder-holder th .check-box").removeClass("active");
        if (activeChecks.length > 1)
            $("#inv-right .inv-btn").show(200);
        else
            $("#inv-right .inv-btn").hide(200);
        e.preventDefault();
    });
    $(document).on("click", "#inv-right .workorder-holder th .check-box", function (e) {
        var self = $(this);
        self.toggleClass("active");
        var isActive = self.is(".active");
        var total = 0;
        if (isActive) {
            $("#inv-right .workorder-holder td .check-box").each(function (i, elm) {
                $(elm).addClass("active");
                total += parseFloat($(elm).parents("tr").find("td:last-child").data("total"));
            });
        }
        else
            $("#inv-right .workorder-holder td .check-box").removeClass("active");

        var totHolder = $("#inv-right .total.tot");
        totHolder.removeClass("pulse");
        totHolder.find("span").text(total.formatMoney(2, "$"));
        totHolder.data("total", total).addClass("pulse");
        if (isActive)
            $("#inv-right .inv-btn").show(200);
        else
            $("#inv-right .inv-btn").hide(200);
    });
    $(document).on("click", "#inv .close", function (e) {
        $("#inv").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
            $(this).remove();
            $(".btm-overlay").fadeOut(200, function () { $(this).remove(); });
        });
    });
    //$(document).on("click", "#inv .inv-process", function (e) {
    function NewInvoiceProcess(ids, masterId, dBox) {

        var code = $("#inv-left ul li.active").data("code");
        if (ids.length > 0) {
            loading(true);
            $.post("../invoice/CheckForValidWOs", { ids: ids }, function (data) {
                if (!data.any || !data.verified) {
                    var title = "<i class='fe-comment'></i> Cannot Process!"
                    var msg = "";
                    removeLoader(true, 500, function () {
                        if (data.msg == "") {
                            if (!data.any)
                                msg = "<p>Cannot <b class='default-col'>Process</b> the Merged Work Orders those have no activities. Please make sure that at least one (1) <b class='act-col'>Activity</b> was created.</p>";
                            else
                                msg = "<p>Cannot <b class='default-col'>Process</b> the Merged Work Orders those have not yet been <b class='act-col'>Verified</b>. Please make sure that you have changed the Work Order's status.</p>";
                        }
                        else
                            msg = "<p>Cannot <b class='default-col'>Process</b>. Please make sure that you have already <b class='default-col'>Processed</b> the Work Orders.</p>";
                        displayDialogMsg(msg, title);
                    });
                }
                else {
                    var invID = $("#inv .bottom select").val();
                    //var buttons = [
                    //            {
                    //                text: "Continue", click: function () {
                    //                    $(this).dialog("close");
                    //                    Saveinv(ids, invID, dBox);
                    //                    //loading(true);
                    //                    //$.post("../invoice/ProcessWorkOrders", { code: code, invID: invID, id: $("#invIdHidden").val(), title: $("#txtTitle").val() }, function (data) {
                    //                    //    removeLoader(true, 500, function () {
                    //                    //        if (typeof data.Additional === 'undefined') {
                    //                    //            $("#inv").remove();
                    //                    //            $("body").append(data);
                    //                    //            $("#inv").show();
                    //                    //        }
                    //                    //        msg = "<p>Successfully <b class='default-col'>Processed</b> Work Orders. </p>";
                    //                    //        var title = "<i class='fe-comment'></i> Success!"
                    //                    //        displayDialogMsg(msg, title);
                    //                    //    });
                    //                    //});
                    //                }
                    //            },
                    //            { text: "Cancel", click: function () { $(this).dialog("close"); } }
                    //];
                    //var msg = '<p>Are you sure you want to <b class="default-col">Process</b> this Selected Work Orders?</p></div>';
                    //removeLoader(true, 500, function () {
                    //    displayConfirmationMsg(msg, 'Process Selected Work Orders', buttons);
                    //});
                    Saveinv(ids, invID, dBox);
                }
            });
        }
        else {
            var title = "<i class='fe-comment'></i> Cannot Process!"
            var msg = "<p>Cannot <b class='default-col'>Process</b>. Please make sure that you have selected the Work Orders.</p>";
            displayDialogMsg(msg, title);
        }
       // e.preventDefault();
    }

    $(document).on("click", ".inv-process", function (e) {
        var rows = $(".workorder-holder table > tbody  > tr");
        if (rows.length < 1) {
            var htmlMsg = "<p>No <b class='loc-col'>Work Order(s)</b> have been selected for <b class='date-col'>Process</b>. First select your desired <b class='loc-col'>Work Order(s)</b> then try again.</p>";
            var htmlTitle = "<i class='fe-comment'></i>Cannot Process";
            displayDialogMsg(htmlMsg, htmlTitle);
        }
        else {
            rows = $.grep(rows, function (row) {
                var self = $(row).find("button");
                var isChecked = $(self).is(".active");
                return isChecked;
            });
            var ids = $.map(rows, function (elm) { return $(elm).attr("id").replace("tr-", ""); });
            var masterId = $("#invIdHidden").val();//Math.min.apply(Math, ids);
            //ids = $.grep(ids, function (id) {
            //    return id != masterId;
            //});
            if (ids.length > 0) {
                var buttons = [
                    { text: "Continue", click: function () { $(this).dialog("close"); NewInvoiceProcess(ids, masterId, $(this)); } },
                    { text: "Cancel", click: function () { $(this).dialog("close"); } }
                ];
                var msg = "<p>Selected <b class='date-col'>Work Orders</b>, will be <b class='default-col'>Process</b>. Are you sure you want to Continue?</p></div>";
                displayConfirmationMsg(msg, 'Process Confirmation', buttons);
            }
            else {
                var htmlMsg = "<p>No <b class='loc-col'>Work Order(s)</b> have been selected for <b class='date-col'>Process</b>. First select your desired <b class='loc-col'>Work Order(s)</b> then try again.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Process";
                displayDialogMsg(htmlMsg, htmlTitle);
            }
        }
        e.preventDefault();
    });
    function Saveinv(ids, masterId, dBox) {        
        if ($("#txtTitle").val() == "") {
            removeLoader(true, 200, function () {
                var htmlMsg = "<p>No <b class='loc-col'>Invoice Title</b> have been inserted for <b class='date-col'>Process</b>. First insert your desired <b class='loc-col'>Title</b> then try again.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Process";
                displayDialogMsg(htmlMsg, htmlTitle);
            });
        }
        else {
            loading(true);
            var isUpdate = $("#invIdHidden").val() != undefined && $("#invIdHidden").val() != "" && $("#invIdHidden").val() != "" ? true : false;
            $.ajax({
                url: "../invoice/ProcessWorkOrders",
                data: $.toDictionary({ wOIds: ids, isUpdate: isUpdate, id: masterId, title: $("#txtTitle").val() }),
                type: "post",
                success: function (r) {
                    removeLoader(true, 200, function () {
                        //$("#inv").remove();
                        //$("body").append(data);
                        //$("#inv").show();
                        var htmlMsg = "";
                        var htmlTitle = "";
                        if (r.Code == "100") {
                            //$("#invIdHidden").val(r.Additional);
                            //htmlMsg = "<p>Work Orders were <b>Processed</b> Successfully!.</p>";
                            //htmlTitle = "<i class='fe-comment'></i> Success";
                            dTable2.draw();
                            $("#inv").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
                                $(this).remove();
                                $(".btm-overlay").fadeOut(200, function () { $(this).remove(); });
                            });                            
                        }
                        else {
                            htmlMsg = r.Msg;
                            htmlTitle = "<i class='fe-comment'></i>Cannot Process";
                            displayDialogMsg(htmlMsg, htmlTitle);
                        }                        
                    });
                    try {
                        dBox.dialog("close");
                    }
                    catch (e) {

                    }
                }
            });
        }
    }
    /*End New invoices*/

});
function LoadMetarialUsagePopup(self) {
    var wid = self.parents("tr").attr("id");
    //wid = wid.toLowerCase().indexOf("invoiced") > -1 ? "invoiced" : wid.split("_")[1];
    //if (wid != "invoiced") {
    //    loading(false);
    //    url = self.parents("table").data("maturl");
    //    $("body").append("<div class='full-overlay'></div>");
    //    $(".full-overlay").fadeIn(200);
    //    $.ajax({
    //        url: url,
    //        data: { wid: wid },
    //        type: "POST",
    //        success: function (html) {
    //            removeLoader(false, 500, function () {
    //                $("body").append(html);
    //                $("#matConstructor [title]").tooltip({ placement: "top" });
    //                $("#matConstructor").addClass("animated bounceInRight").show();
    //                $('.mat-const-main .const-holder ul').perfectScrollbar();
    //                $("#matConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
    //                $("#matConstructor .title span").dotdotdot();
    //                var actHolder = $(".mat-const-main .const-holder.const-mats");
    //                if (actHolder.find("li").length == 0) {
    //                    actHolder.find("ul").remove();
    //                    $(".mat-const-main .const-mats .placeholder").show(100, function () {
    //                        $(".mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
    //                    });
    //                }
    //                else {
    //                    var ph = actHolder.find(".placeholder");
    //                    var time = ph.is(":visible") ? 300 : 10;
    //                    ph.find("button").removeClass("activated animated flash");
    //                    ph.hide(200);
    //                    setTimeout(function () {
    //                        setDisableMetarialOptions();
    //                        actHolder.find("select").select2({ width: "resolve" });
    //                        actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
    //                        $('.mat-const-main .const-mats ul').perfectScrollbar();
    //                    }, time);
    //                }
    //            });
    //        }
    //    });
    //}
    //else {
    //    var htmlMsg = "<p>This Workorder has already been <b>Invoiced</b>. To edit the Workorder you must first reverse the Invoice.</p>";
    //    var htmlTitle = "<i class='fe-comment'></i>Already Processed";
    //    displayDialogMsg(htmlMsg, htmlTitle);
    //}
}


function LoadMergeWOs(code, id) {
    var self = null;
    var val = code;
    var holder = $("#merger-left ul");
    $("#merger-left ul").perfectScrollbar('destroy');
    $.get("workorder/GetMergeReferencesForMerged", { val: val, id: id }, function (data) {
        holder.html(data);
        $("#merger-left ul").perfectScrollbar();
        var items = $("#merger-left ul li");
        for (var i = 0; i < items.length; i++) {
            if ($(items[i]).data("code").toLowerCase() == code.toLowerCase()) {
                self = $(items[i]);
                break;
            }
        }
        LoadMergedGridWithMerged(self, id);
    });
}
function LoadMergedGridWithMerged(self, id) {
    $("#merger-left ul li:not(.empty)").removeClass("active");
    var val = self.data("code");
    self.addClass("active");
    var holder = $("#merger-right .workorder-holder");
    $("#merger-right .center").perfectScrollbar('destroy');
    $.get("workorder/MergeCandiditatesWithMerged", { code: val, id: id }, function (data) {
        holder.html(data);
        $("#merger-right .total.tot").data("total", 0).find("span").text("$0.00");
        $("#merger-right .center").perfectScrollbar();
        CalculateTotal();
    });
}
function LoadMergedGrid(self) {
    $("#merger-left ul li:not(.empty)").removeClass("active");
    var val = self.data("code");
    self.addClass("active");
    var holder = $("#merger-right .workorder-holder");
    $("#merger-right .center").perfectScrollbar('destroy');
    $.get("workorder/MergeCandiditates", { code: val }, function (data) {
        holder.html(data);
        $("#merger-right .total.tot").data("total", 0).find("span").text("$0.00");
        $("#merger-right .center").perfectScrollbar();
        CalculateTotal();
    });
}
function CalculateTotal() {
    var totHolder = $("#merger-right .total.tot");
    totHolder.removeClass("pulse");
    var total = 0;
    $('.workorder > tbody  > tr').each(function () {
        var row = $(this);
        if(row.find(".check-box").is(".active"))
            total += parseFloat(row.find("td:last-child").data("total"));
    });
    totHolder.find("span").text(total.formatMoney(2, "$"));
    totHolder.data("total", total).addClass("pulse");
    var tds = $("#merger-right .workorder-holder td");
    var activeChecks = tds.find(".check-box.active")
    if (tds.find(".check-box").length === activeChecks.length)
        $("#merger-right .workorder-holder th .check-box").addClass("active");
    else
        $("#merger-right .workorder-holder th .check-box").removeClass("active");
    if (activeChecks.length > 1)
        $("#merger-right .merger-btn").show(200);
    else
        $("#merger-right .merger-btn").hide(200);
}
function deleteMergeRow(elm) {
    var url = "workorder/DeleteMerge";
    loading(true);
    $.post(url, { id: elm.parents("tr").attr("id") }, function (data) {
        removeLoader(true, 300, function () {
            if (data.Additional > 0) {
                $(".tel-confirm-dialog").dialog("close");
                var page = dTable2.page();
                dTable2.page(page).draw(false);
                //dTable2.draw();
            }
            else {
                $(".tel-confirm-dialog").dialog("close");
                var htmlMsg = "<p>This record cannot be <b class='default-col'>Deleted</b> because it is still <b>Linked</b> to other records in the Database.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Delete";
                displayDialogMsg(htmlMsg, htmlTitle);
            }
        });
    });
}

function LoadInvWOs(code, id) {
    var self = null;
    var val = code;
    var holder = $("#inv-left ul");
    $("#inv-left ul").perfectScrollbar('destroy');
    $.get("invoice/GetMergeReferencesForMerged", { val: val, id: id }, function (data) {
        holder.html(data);
        $("#inv-left ul").perfectScrollbar();
        var items = $("#inv-left ul li");
        for (var i = 0; i < items.length; i++) {
            if ($(items[i]).data("code").toLowerCase() == code.toLowerCase()) {
                self = $(items[i]);
                break;
            }
        }
        LoadInvGridWithInv(self, id);
    });
}
function LoadInvGridWithInv(data, id) {
    //$("#inv-left ul li:not(.empty)").removeClass("active");
    //var val = self.data("code");
    //self.addClass("active");
    var holder = $("#inv-right .workorder-holder");
    $("#inv-right .center").perfectScrollbar('destroy');

    $.get("invoice/InvoiceCandiditatesWithInvoiced", { model: data, id: id }, function (data) {
        holder.html(data);
        $("#inv-right .total.tot").data("total", 0).find("span").text("$0.00");
        $("#inv-right .center").perfectScrollbar();
        CalculateWOTotal();
    });
}
function LoadInvGrid(self) {
    $("#inv-left ul li:not(.empty)").removeClass("active");
    var val = self.data("code");
    self.addClass("active");
    var holder = $("#inv-right .workorder-holder");
    $("#inv-right .center").perfectScrollbar('destroy');
    $.get("invoice/MergeCandiditates", { code: val }, function (data) {
        holder.html(data);
        $("#inv-right .total.tot").data("total", 0).find("span").text("$0.00");
        $("#inv-right .center").perfectScrollbar();
        CalculateWOTotal();
    });
}
function CalculateWOTotal() {
    var totHolder = $("#inv-right .total.tot");
    totHolder.removeClass("pulse");
    var total = 0;
    $('.workorder > tbody  > tr').each(function () {
        var row = $(this);
        if (row.find(".check-box").is(".active"))
            total += parseFloat(row.find("td:last-child").data("total"));
    });
    totHolder.find("span").text(total.formatMoney(2, "$"));
    totHolder.data("total", total).addClass("pulse");
    var tds = $("#inv-right .workorder-holder td");
    var activeChecks = tds.find(".check-box.active")
    if (tds.find(".check-box").length === activeChecks.length)
        $("#inv-right .workorder-holder th .check-box").addClass("active");
    else
        $("#inv-right .workorder-holder th .check-box").removeClass("active");
    if (activeChecks.length > 1)
        $("#inv-right .inv-btn").show(200);
    else
        $("#inv-right .inv-btn").hide(200);
}
function deleteinvow(elm) {
    var url = "invoice/DeleteMerge";
    loading(true);
    $.post(url, { id: elm.parents("tr").attr("id") }, function (data) {
        removeLoader(true, 300, function () {
            if (data.Additional > 0) {
                $(".tel-confirm-dialog").dialog("close");
                var page = dTable2.page();
                dTable2.page(page).draw(false);
                //dTable2.draw();
            }
            else {
                $(".tel-confirm-dialog").dialog("close");
                var htmlMsg = "<p>This record cannot be <b class='default-col'>Deleted</b> because it is still <b>Linked</b> to other records in the Database.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Delete";
                displayDialogMsg(htmlMsg, htmlTitle);
            }
        });
    });
}


function setValidationMsgsForForm(form) {
    form.find(".field-validation-error").each(function () {
        var self = $(this);
        var msg = self.find("span").text();
        if (self.parents("td").length > 0)
            self.parents("td").find("input, select").attr("title", msg).tooltip({ placement: "top" });
        else
            self.parent().find("input, select").attr("title", msg).tooltip({ placement: "top" });
    });
}
function setValidationMsgsForElm(elm) {
    var parent = elm.parents("td").length == 0 ? elm.parent() : elm.parents("td");
    var msg = parent.find(".field-validation-error").find("span").text();
    elm.attr("title", msg).tooltip({ placement: "top" });
}

function deleteSWORow(elm) {
    var url = deleteUrl == null ? elm.parents("table").data("url") + "/deleteSWO" : deleteUrl;
    loading(true);
    $.post(url, { id: elm.parents("tr").attr("id") }, function (data) {
        removeLoader(true, 300, function () {
            if (data.Additional > 0) {
                $(".tel-confirm-dialog").dialog("close");
                var page = dTable.page();
                dTable.page(page).draw(false);
                //dTable.draw();
            }
            else {
                $(".tel-confirm-dialog").dialog("close");
                var htmlMsg = "<p>This record cannot be <b class='default-col'>Deleted</b> because it is <b>not in</b> New status.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Delete";
                displayDialogMsg(htmlMsg, htmlTitle);
            }
        });
    });
}
function displayDialogMsg(htmlMsg, htmlTitle) {
    var html = '<div class="tel-dialog" title="' + htmlTitle + '">' + htmlMsg + '</div>';
    $("body").append(html);
    $(".tel-dialog").dialog(dialogOptions);
}
function displayConfirmationMsg(htmlMsg, title, buttons) {
    var tempOptions = $.extend(true, {}, dialogOptions);
    tempOptions.dialogClass = "animated wobble";
    tempOptions.buttons = buttons;
    var html = '<div class="tel-confirm-dialog" title="<i class=fe-flash></i>' + title + '">' + htmlMsg + '</div>';
    $("body").append(html);
    $(".tel-confirm-dialog").dialog(tempOptions);
}
function applyDateTemplate(date, classes) {
    return '<li data-date="' + date + '" ' + (classes != null ? ' class="' + classes + '">' : '>') +
           '<span>' + Date.parse(date).toString("MMMM dd, yyyy") + '</span>' +
           '<div class="wo-actions">' +
           '<a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a><a href="#"  class="edit-action" title="Edit"><i class="fe-pencil-2"></i></a>' +
           '</div></li>';
}
function applyLocationTemplate(loc, classes, comment) {
    return '<li class="clearfix ' + (classes != null ? classes : '') + '">' +
           '<span>' + loc + '</span><span style="display:none;">' + comment + '</span>' +
                '<div class="wo-actions">' +
                    '<a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a>' +
                    '<a href="#" class="edit-action" title="Edit"><i class="fe-pencil-2"></i></a>' +
                '</div></li>';
}
function applyActivityTemplate(acts) {
    var actBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
    for (x = 0; x < acts.length; x++) {
        var act = acts[x];
        actBox += '<option value="' + act.activityID + '">' + act.description + '</option>';
    }
    actBox += '</select><input type="text" class="act-amt" /></div>' +
        '<div class="wo-actions"><a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a></div></li>';
    return actBox;
}
function applyActivityMatTemplate(acts) {
    var actBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
    for (x = 0; x < acts.length; x++) {
        var act = acts[x];
        actBox += '<option value="' + act.activityID + '">' + act.description + '</option>';
    }
    actBox += '</select><input type="text" class="act-amt"/></div>';
    var matReq = $("#matsReqHidden").val();
    actBox += (matReq == "true" ? '' : '<div class="wo-actions"><a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a></div></li>');
    return actBox;
}

function applyQuotActivityTemplate(acts) {
    var actBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
    for (x = 0; x < acts.length; x++) {
        var act = acts[x];
        actBox += '<option value="' + act.activityID + '">' + act.description + '</option>';
    }
    actBox += '</select><input type="text" class="act-rat" style="width: 70px;"/><input type="text" class="act-amt" /></div>' +
        '<div class="wo-actions"><a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a></div></li>';
    return actBox;
}
function applySInvActivityTemplate(acts) {
    var actBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
    for (x = 0; x < acts.length; x++) {
        var act = acts[x];
        actBox += '<option value="' + act.activityID + '">' + act.description + '</option>';
    }
    actBox += '</select><input type="text" class="act-rat" style="width: 70px;"/><input type="text" class="act-amt" /></div>' +
        '<div class="wo-actions"><a href="#" class="del-action" title="Delete"><i class="fe-trash"></i></a></div></li>';
    return actBox;
}

function applyActivityTemplateWithValues(acts, actId, actAmt) {
    
    var actBox = '<li class="clearfix new-act"><div class="act-holder"><select class="act-ddl" name="acts"><option></option>';
    for (x = 0; x < acts.length; x++) {
        var act = acts[x];
        actBox += '<option value="' + act.activityID + '" ' + (act.activityID == actId ? 'selected="selected"' : '') + '>' + act.description + '</option>';
    }
    actBox += '</select><input type="text" class="act-amt" value="' + actAmt + '" /></div>' +
        '<div class="wo-actions"><a href="#" class="del-action"><i class="fe-trash" title="Delete"></i></a></div></li>';
    return actBox;
}
function discardDataCheck(type) {
    if (wochanged)
        return true;
    switch (type) {
        case "full":
            return $("#constructor .const-holder ul li[class*='new-']").length > 0;
            break;
        case "acts":
            return $("#constructor .const-locs ul li.new-loc, #constructor .const-acts ul li.new-act").length > 0;
            break;
        case "locs":
            return $("#constructor .const-acts ul li.new-act").length > 0;
            break;
    }
}
function generateLocations() {
    $("#constructor .const-holder.const-dates ul li").removeClass("active");
    $(".const-main .const-acts .placeholder").show(200).find("button").removeClass("activated");
    $(".const-main .const-acts ul").remove();
    var self = temp;
    self.addClass("active");
    var locHolder = $(".const-main .const-holder.const-locs");
    if (self.hasClass("new-date")) {
        var data = {
            wid: $("#constructor").data("id"),
            invID: $("#constructor").data("inv"),
            date: self.data("date"),
            type: $("#constructor").hasClass("view-wo") ? "view" : ""
        };
        locHolder.find("ul").remove();
        $(".const-main .const-locs .placeholder").show(100, function () {
            $(".const-main .const-locs .placeholder button").addClass("activated animated flash");
        });
        if (!AddLocationsFromArry(data)) {

        }
    }
    else {
        var data = {
            wid: $("#constructor").data("id"),
            invID: $("#constructor").data("inv"),
            date: self.data("date"),
            type: $("#constructor").hasClass("view-wo") ? "view" : ""
        };
        $.post("/workorder/getconstructorlocations", data, function (html) {
            var ph = locHolder.find(".placeholder");
            var time = ph.is(":visible") ? 300 : 10;
            ph.find("button").removeClass("activated animated flash")
            ph.hide(200);
            setTimeout(function () {
                locHolder.find("ul").remove();
                locHolder.append(html);
                AddLocationsFromArry(data);
                $('.const-main .const-locs ul').perfectScrollbar();
            }, time);
        });
    }
}
function generateActivities() {
    $("#constructor .const-holder.const-locs ul li").removeClass("active");
    var self = temp;
    self.addClass("active");
    var actHolder = $(".const-main .const-holder.const-acts");
    if (self.hasClass("new-loc")) {
        var data = {
            wid: $("#constructor").data("id"),
            invID: $("#constructor").data("inv"),
            date: $("#constructor .const-holder.const-dates ul li.active").data("date"),
            location: $(self.find("span")[0]).text(),
            type: $("#constructor").hasClass("view-wo") ? "view" : ""
        };
        actHolder.find("ul").remove();
        $(".const-main .const-acts .placeholder").show(100, function () {
            $(".const-main .const-acts .placeholder button").addClass("activated animated flash");
        });
        if (!AddActivitiesFromArry(data)) {
            //actHolder.find("ul").remove();
            
        }
    }
    else {
        var data = {
            wid: $("#constructor").data("id"),
            invID: $("#constructor").data("inv"),
            date: $("#constructor .const-holder.const-dates ul li.active").data("date"),
            location: self.find("span.loc-details").text(),
            type: $("#constructor").hasClass("view-wo") ? "view" : ""
        };
        if (data.location == "")
            data.location = $(self.find("span")[0]).text();
        $.post("/workorder/getconstructoractivities", data, function (html) {
            var ph = actHolder.find(".placeholder");
            var time = ph.is(":visible") ? 300 : 10;
            ph.find("button").removeClass("activated animated flash");
            ph.hide(200);
            setTimeout(function () {
                
                actHolder.find("ul").remove();
                actHolder.append(html);
                AddActivitiesFromArry(data);
                setDisableActivityOptions();
                actHolder.find("select").select2({ width: "resolve" });
                actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                $('.const-main .const-acts ul').perfectScrollbar();
            }, time);
        });
    }
}
function AddActivitiesFromArry(data) {    
    var actAvailable = false;
    var actsHolder = $("#constructor > .const-main .const-acts");
    if (CheckIsActExist(data.location, data.date)) {
        
        if (actsHolder.find("ul").length == 0)
            actsHolder.append("<ul></ul>");
        var ph = $("#constructor .const-acts .placeholder");
        var time = ph.is(":visible") ? 300 : 10;
        ph.hide(200);
    }
    for (var i = 0; i < tempActs.length; i++) {
        if (tempActs[i].location == data.location && tempActs[i].actDate == data.date && !CheckIsInPlaceHolder(tempActs[i].actID)) {
            actsHolder.find("ul").append(applyActivityTemplateWithValues(actsTempStorage, tempActs[i].actID, tempActs[i].actQty));
            $('.const-main .const-acts ul').perfectScrollbar("update");
            setDisableActivityOptions();
            actsHolder.find("select").select2({ width: "resolve", placeholder: "Select an Activity" });
            actsHolder.find("input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
            actAvailable = true;
        }
    }
    return actAvailable;
}
function AddLocationsFromArry(data) {
    var locAvailable = false;
    var locsHolder = $("#constructor > .const-main .const-locs");
    if (CheckIsLocExist(data.location, data.date)) {
        if (locsHolder.find("ul").length == 0)
            locsHolder.append("<ul></ul>");
        var ph = $("#constructor .const-locs .placeholder");
        var time = ph.is(":visible") ? 300 : 10;
        ph.hide(200);
    }
    for (var i = 0; i < tempLocs.length; i++) {
        if (tempLocs[i].actDate == data.date && !CheckIsLocInPlaceHolder(tempLocs[i].location)) {
            locsHolder.find("ul").append(applyLocationTemplate(tempLocs[i].location, "new-loc", tempLocs[i].comment));
            $('.const-main .const-locs ul').perfectScrollbar("update");
            locAvailable = true;
        }
    }
    return locAvailable;
}
function CheckIsActExist(loc, date) {
    var isExist = false;
    for (var i = 0; i < tempActs.length; i++) {
        if (tempActs[i].location == loc && tempActs[i].actDate == date) {
            isExist = true;
            break;
        }
    }
    return isExist;
}
function CheckIsLocExist(date) {
    var isExist = false;
    for (var i = 0; i < tempLocs.length; i++) {
        if (tempLocs[i].actDate == date) {
            isExist = true;
            break;
        }
    }
    return isExist;
}
function CheckIsInPlaceHolder(actId) {
    var inPlace = false;
    //var loc = $($(this).find("span")[0]).text();
    //var actDate = $(".const-main .const-holder.const-dates ul li.active").data("date");
    $(".const-main .const-holder.const-acts ul li").each(function () {
        var self = $(this);
        var actID = self.find("select").val();
        if (actId == actID) {
            inPlace = true;
        }
    });
    return inPlace;
}
function CheckIsLocInPlaceHolder(loc) {
    var inPlace = false;
    //var loc = $($(this).find("span")[0]).text();
    //var actDate = $(".const-main .const-holder.const-dates ul li.active").data("date");
    $(".const-main .const-holder.const-locs ul li").each(function () {
        var self = $(this);
        var lc = $(self.find("span")[0]).text();
        if (loc == lc) {
            inPlace = true;
        }
    });
    return inPlace;
}
function setupLocationDialogBox(val, adtlDetail) {
    var locsHolder = $("#constructor > .const-main .const-locs");
    var tempOptions = $.extend(true, {}, dialogOptions);
    tempOptions.dialogClass = "animated rubberBand";
    tempOptions.buttons = [{
        text: "Save", click: function () {
            var locElm = $(this).find("textarea.text-loc");
            var loc = locElm.val();
            var commentElm = $(this).find("textarea.text-comment");
            var commentdetil = commentElm.val();
            if (loc.isEmpty()) {
                locElm.css("border-color", "red");
            }
            if (commentdetil.isEmpty()) {
                commentElm.css("border-color", "red");
            }
            else {
                var ph = $("#constructor .const-locs .placeholder");
                var time = ph.is(":visible") ? 300 : 10;
                ph.hide(200);
                var loc = $(this).find("textarea.text-loc").val();
                var comment = $(this).find("textarea.text-comment").val();
                var wid = $("#constructor").data("id");
                var date = $("#constructor .const-dates li.active").data("date");
                var oldLoc = temp.find("span").text();
                var newLoc = loc;
                setTimeout(function () {
                    if (locElm.hasClass("editing")) {
                        $.post("/workorder/UpdateWorkOrderLocation", { wid: wid, date: date, newLocation: newLoc, oldLocation: val, aditinalDetail: comment }, function () {
                            temp.replaceWith(applyLocationTemplate(loc, temp.attr("class"), comment));
                        });
                    }
                    else {
                        if (locsHolder.find("ul").length == 0)
                            locsHolder.append("<ul></ul>");

                        $.post("/workorder/UpdateWorkOrderLocation", { wid: wid, date: date, newLocation: newLoc, oldLocation: "newrec", aditinalDetail: comment }, function () {
                            //temp.replaceWith(applyLocationTemplate(loc, temp.attr("class"), comment));
                        });
                        locsHolder.find("ul").append(applyLocationTemplate(loc, "new-loc", comment));
                    }
                    $('.const-main .const-locs ul').perfectScrollbar("update");
                }, time);
                $(this).dialog("close");
            }
        }
    }];
    $("body").append('<div class="const-loc-modal" title="<i class=fe-location-2></i> Add Location">' +
        '<lable style="float: left;padding-bottom: 5px;padding-left: 10px;">Location</lable><br /><textarea class="text-loc' + (val != "" ? ' editing"' : '"') + '>' + val + '</textarea>' +
        '<lable style="float: left;padding-bottom: 5px;padding-left: 10px;">Comment</lable><br /><textarea class="text-comment' + (adtlDetail != "" ? ' editing"' : '"') + '>' + adtlDetail + '</textarea></div>');
    $(".const-loc-modal").dialog(tempOptions);
}
function setupActivityCalendar(date) {
    var dates = [];
    var datesHolder = $("#constructor > .const-main .const-dates");
    datesHolder.find("ul li").each(function () {
        var dt = $(this).data("date");
        dates.push(Date.parse(dt).toString("M/dd/yyyy"));
    });
    var tempOptions = $.extend(true, {}, dialogOptions);
    tempOptions.dialogClass = "animated rubberBand";
    tempOptions.buttons = [{
        text: "Save", click: function () {
            var dateObj = $(".const-date-modal .cal");
            var dt = dateObj.datepicker('getDate');
            var dtIndex = dt.toString().indexOf(" (");
            var parsedDt = dt.toString().substr(0, dtIndex);
            var newDate = Date.parse(parsedDt).toString("M/dd/yyyy HH:mm");
            var ph = $("#constructor .const-dates .placeholder");
            var time = ph.is(":visible") ? 300 : 10;
            ph.hide(200);
            var wid = $("#constructor").data("id");
            var newDate = newDate;

            setTimeout(function () {
                if (dateObj.hasClass("editing")) {
                    var oldDate = temp.data("date");
                    $.post("/workorder/UpdateWorkOrderDate", { wid: wid, newDate: newDate, oldDate: oldDate }, function () {
                        temp.replaceWith(applyDateTemplate(newDate, temp.attr("class")));
                    });
                }
                else {
                    if (datesHolder.find("ul").length == 0)
                        datesHolder.append("<ul></ul>");
                    datesHolder.find("ul").append(applyDateTemplate(newDate, "new-date"));
                }
                $('.const-main .const-dates ul').perfectScrollbar("update");
            }, time);
            $(this).dialog("close");
        }
    }];
    tempOptions.width = 318;
    $("body").append('<div class="const-date-modal" title="<i class=fe-calendar></i> Add Date">' +
        '<div class="cal' + (date != null ? ' editing"' : '"') + '></div></div>');
    var nextAvailableDay = Date.today().toString("M/dd/yyyy");
    var dindex = $.inArray(nextAvailableDay, dates);
    while (dindex > -1) {
        nextAvailableDay = Date.parse(nextAvailableDay).addDays(1).toString("M/dd/yyyy");
        dindex = $.inArray(nextAvailableDay, dates);
    }
    if (date == null)
        date = nextAvailableDay;
    else {
        delete dates[$.inArray(Date.parse(date).toString("M/dd/yyyy"), dates)];
        date = Date.parse(date).toString("M-d-yyyy");
    }
    $(".const-date-modal .cal").datepicker({
        todayHighlight: true,
        format: "D M dd, yyyy",
        beforeShowDay: function (date) {
            var d = Date.parse(date.toDateString()).toString("M/dd/yyyy");
            return $.inArray(d, dates) == -1;
        }
    }).datepicker('setDate', date);
    $(".const-date-modal").dialog(tempOptions);
}
function setDisableActivityOptions() {
    actsSelectedValue = [];
    var selects = $("#constructor .const-acts .act-holder select.act-ddl");
    selects.find("option.elsewhere").removeAttr("disabled");
    selects.each(function () {
        var val = $(this).val();
        actsSelectedValue.push(val);
    });
    for (var x = 0; x < actsSelectedValue.length; x++) {
        var val = actsSelectedValue[x];
        selects.each(function () {
            var self = $(this);
            var option = self.find("option[value='" + val + "']");
            if (self.val() != val) {
                option.addClass("elsewhere").attr("disabled", "disabled");
            }
            else {
                option.removeClass("elsewhere")
            }
        });
    }
}
function deleteAction(type) {
    var val = type == "Date" ? temp.data("date") : type == "Location" ? temp.find("span.loc-details").text() : temp.data("id");
    var invid = $("#constructor").data("inv");
    var wid = $("#constructor").data("id");
    $.post("/workorder/deletefromworkorder", { val: val, type: type, wid: wid, invid: invid }, function (data) {
        if (data.Code == "100") {
            temp.remove();
            if (type == "Activity") {
                var actHolder = $(".const-main .const-holder.const-acts");
                if (actHolder.find("li").length < 1) {
                    $(".const-main .const-holder.const-locs li.active").addClass("new-loc");
                    actHolder.find("ul").remove();
                    actHolder.find(".placeholder").show(100, function () {
                        $(this).find("button").addClass("activated animated flash");
                    });
                }
            }
            else if (type == "Location") {
                var locHolder = $(".const-main .const-holder.const-locs");
                var actHolder = $(".const-main .const-holder.const-acts");
                actHolder.find("ul").remove();
                actHolder.find(".placeholder").show(100);
                if (locHolder.find("li").length < 1) {
                    $(".const-main .const-holder.const-dates li.active").addClass("new-date");
                    locHolder.find("ul").remove();
                    locHolder.show(100, function () {
                        locHolder.find("button").addClass("activated animated flash");
                    });
                }
            }
            else {
                var holders = $(".const-main .const-holder.const-locs, .const-main .const-holder.const-acts");
                holders.find("ul").remove();
                holders.find(".placeholder").show(100);
            }
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
        $('.const-main .const-holder ul').perfectScrollbar("update");
    });
}
function updateAction() {
    var actDate = $(".const-main .const-holder.const-dates ul li.active").data("date");
    var actLoc = $($(".const-main .const-holder.const-locs ul li.active").find("span")[0]).text();
    var actLocComment = $($(".const-main .const-holder.const-locs ul li.active").find("span")[1]).text();
    var wID = $("#constructor").data("id");
    var invID = $("#constructor").data("inv");
    var acts = [];
    $(".const-main .const-holder.const-acts ul li").each(function () {
        var self = $(this);
        var actID = self.find("select").val();
        var actQty = self.find(".act-amt").val();
        var matsReq = self.find(".mats-req") != undefined ? self.find(".mats-req").val() : false;
        var mats = [];
        for (var m = 0; m < actMaterialQtyArry.length; m++) {
            if (new Date(actMaterialQtyArry[m].date).toLocaleDateString() == new Date(actDate).toLocaleDateString() && actMaterialQtyArry[m].loc == actLoc && actMaterialQtyArry[m].actID == actID) {
                var matRID = actMaterialQtyArry[m].RID;
                var matId = actMaterialQtyArry[m].matID;
                var matQty = actMaterialQtyArry[m].matQty;
                mats.push({
                    RID: matRID,
                    MetarialId: matId,
                    Amount: matQty
                });
            }
        }
        if (matsReq == "true" && mats.length <= 0)
            return;
        acts.push({
            RecID: self.data("id"),
            RID: actID,
            Amount: actQty,
            HasMetarials: mats.length > 0 ? true : false,
            Metarials: mats
        });
    });
    var data = [];
    data.push({
        WID: wID,
        InvId: invID,
        Date: actDate,
        Location: actLoc,
        AdtnlDetails: actLocComment,
        Activities: acts        
    });
    $(".const-main .const-holder.const-locs ul li").each(function () {
        var loc = $($(this).find("span")[0]).text();
        var locComment = $($(this).find("span")[1]).text();
        if (loc != actLoc) {
            var actsOthr = [];
            for (var i = 0; i < tempActs.length; i++) {
                if (tempActs[i].location == loc && tempActs[i].actDate == actDate) {
                    var id = tempActs[i].id;
                    var actID = tempActs[i].actID;
                    var actQty = tempActs[i].actQty;
                    var actMatsReq = tempActs[i].matsReq;
                    var mats = [];
                    for (var m = 0; m < actMaterialQtyArry.length; m++) {
                        if (actMaterialQtyArry[m].date == actDate && actMaterialQtyArry[m].loc == loc && actMaterialQtyArry[m].actID == actID) {
                            var matRID = actMaterialQtyArry[m].RID;
                            var matId = actMaterialQtyArry[m].matID;
                            var matQty = actMaterialQtyArry[m].matQty;
                            mats.push({
                                RID: matRID,
                                MetarialId: matId,
                                Amount: matQty
                            });
                        }
                    }
                    if (actMatsReq == "true" && mats.length <= 0)
                        return;
                    actsOthr.push({
                        RecID: id,
                        RID: actID,
                        Amount: actQty,
                        HasMetarials: mats.length > 0 ? true : false,
                        Metarials : mats
                    });
                }
            }
            if (actsOthr.length > 0)
                data.push({
                    WID: wID,
                    InvId: invID,
                    Date: actDate,
                    Location: loc,
                    AdtnlDetails: locComment,
                    Activities: actsOthr
                });
        }
    });
    $.post("/workorder/updateactivities", $.toDictionary(data), function (data) {
        if (data.Code == "100") {
            $("#constructor").find("li").removeClass("new-date new-loc");
            temp = $("#constructor .const-holder.const-locs ul li.active");
            generateActivities();
            var msg = "<p>Activities were <b>Updated</b> Successfully!.</p>";
            var title = "<i class='fe-comment'></i> Success";
            actMaterialQtyArry = [];
            tempActs = [];
            //closeConstructor();
            displayDialogMsg(msg, title);
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
    });
    
}
function setupMsgBar(type, msg) {
    var mb = $(".tel-form .msg-bar");
    switch (type) {
        case 1:
            mb.find("i").removeClass("fe-cancel fe-ok").addClass("fe-info-1");
            mb.removeClass("success error").find("span").text(msg);
            mb.addClass("animated pulse");
            break;
        case 2:
            mb.find("i").removeClass("fe-cancel fe-info-1").addClass("fe-ok");
            mb.removeClass("error").addClass("success").find("span").text(msg);
            mb.addClass("animated pulse");
            break;
        default:
            mb.find("i").removeClass("fe-ok fe-info-1").addClass("fe-cancel");
            mb.removeClass("success").addClass("error").find("span").text(msg);
            mb.addClass("animated wobble");
            break;
    }
}
function imageExists(image_url) {
    var http = new XMLHttpRequest();
    http.open('HEAD', image_url, false);
    http.send();
    return http.status != 404;
}
function closeConstructor() {
    $("#constructor").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
        $(this).remove();  
        $(".full-overlay").fadeOut(200, function () { $(this).remove(); });
        if (!$("#con-wo").is(":visible") || $(".tech-tbl").length > 0) {
            var page = dTable.page();
            dTable.page(page).draw(false);
        }
        else {
            var page = aDtable.page();
            aDtable.page(page).draw(false);
            $.get("contractor/refreshConStats", { id: $("#sub-head").data("id") }, function (html) {
                $("#sub-head").replaceWith(html);
            });
        }
    });
}
function loading(above) {
    if (!$(".for-loader").is(":visible")) {
        $("body").append('<div class="full-overlay for-loader' + (above ? ' above' : '') + '"></div>');
        $(".for-loader").fadeIn(100);
        $("body").append('<div class="loading-holder spin"></div><span class="loading-t">T</span>');
    }
}
function removeLoader(hidelb, time, callback) {
    first = time <= 1000 ? 1000 : time - 1000;
    next = time <= 1000 ? time * 2 : time;
    setTimeout(function () {
        $(".loading-holder, .loading-t").fadeOut(200, function () { $(this).remove() });
        $(".for-loader" + (hidelb ? ".above" : "")).fadeOut(200, function () { $(this).remove() });
    }, first);
    setTimeout(function () {
        if (callback && (typeof callback == "function")) {
            callback();
        }
    }, next)
}
function setupTechPrintOuts(options) {
    techTable = $("table.tech-tbl").DataTable(options);
    $(".sub-top.tech-prt").append('<div id="tech-range" class="input-daterange input-group clearfix" id="datepicker">' +
        '<input type="text" class="input-sm form-control" name="start" readonly />' +
        '<span class="input-group-addon">to</span>' +
        '<input type="text" class="input-sm form-control" name="end" readonly />' +
        '<button class="refresh"><i class="fe-cw-outline"></i></button>' +
        '<button class="cancel"><i class="fe-cancel"></i></button></div>');
    $(".dataTables_wrapper select").select2({ minimumResultsForSearch: -1, width: "resolve" });
    $('.sub-top.tech-prt .input-daterange').datepicker({
        format: "DD M dd, yyyy"
    });
}
function setupTeamActions(all) {
    $('.team-setup').find(".number").onlyNumbers({ neagtive: false, decimal: true });
    var selects = all ? $('.team-setup tr .con-ddl') : $('.team-setup tr').last().find('.con-ddl');
    selects.select2({
        placeholder: 'select a contractor',
        width: 260,
        ajax: {
            url: "/workorder/getcontractors",
            dataType: 'json',
            delay: 100,
            data: function (params) {
                var names = [];
                $('.team-setup .con-ddl').each(function () {
                    names.push($(this).val());
                });
                return $.toDictionary({
                    ids: names,
                    query: params.term
                });
            },
            processResults: function (data, page) {
                return { results: data.suggestions }
            }
        },
        escapeMarkup: function (markup) { return markup; },
        templateResult: function (exercise) {
            return "<div class='select2-user-result'>" + exercise.term + "</div>";
        },
        templateSelection: function (exercise) {
            return exercise.term;
        },
        formatNoMatches: function (term) {
            return "<b>No Matches Found</b>"
        },
        initSelection: function (element, callback) {
            var elementText = $(element).attr('data-init-text');
            callback({ "term": elementText });
        }
    });
}
function deleteRow(elm) {
    var url = deleteUrl == null ? elm.parents("table").data("url") + "/delete" : deleteUrl;
    loading(true);
    $.post(url, { id: elm.parents("tr").attr("id") }, function (data) {
        removeLoader(true, 300, function () {
            if (data.Additional > 0) {
                $(".tel-confirm-dialog").dialog("close");
                var page = dTable.page();
                dTable.page(page).draw(false);
                //dTable.draw();
            }
            else {
                $(".tel-confirm-dialog").dialog("close");
                var htmlMsg = "<p>This record cannot be <b class='default-col'>Deleted</b> because it is still <b>Linked</b> to other records in the Database.</p>";
                var htmlTitle = "<i class='fe-comment'></i>Cannot Delete";
                displayDialogMsg(htmlMsg, htmlTitle);
            }
        });
    });
}

/*Meterial Constructor */
function discardMetarialDataCheck(type) {
    if (wochanged)
        return true;
    switch (type) {
        case "full":
            return $("#matConstructor .const-holder ul li[class*='new-']").length > 0;
            break;
        case "acts":
            return $("#matConstructor .const-locs ul li.new-loc, #matConstructor .const-mats ul li.new-act").length > 0;
            break;
        case "locs":
            return $("#matConstructor .const-mats ul li.new-act").length > 0;
            break;
    }
}
function setDisableMetarialOptions() {
    actsSelectedValue = [];
    var selects = $("#matConstructor .const-mats .act-holder select.act-ddl");
    selects.find("option.elsewhere").removeAttr("disabled");
    selects.each(function () {
        var val = $(this).val();
        actsSelectedValue.push(val);
    });
    for (var x = 0; x < actsSelectedValue.length; x++) {
        var val = actsSelectedValue[x];
        selects.each(function () {
            var self = $(this);
            var option = self.find("option[value='" + val + "']");
            if (self.val() != val) {
                option.addClass("elsewhere").attr("disabled", "disabled");
            }
            else {
                option.removeClass("elsewhere")
            }
        });
    }
}
function deleteMetarial(type) {
    var val = temp.data("id");
    var invid = $("#matConstructor").data("inv");
    var wid = $("#matConstructor").data("id");
    if (val != undefined && val != 0 && val != -1) {
        $.post("/workorder/deletemetarialfromworkorder", { val: val, type: type, wid: wid, invid: invid }, function (data) {
            if (data.Code == "100") {
                temp.remove();
                if (type == "Activity") {
                    var actHolder = $(".mat-const-main .const-holder.const-mats");
                    if (actHolder.find("li").length < 1) {
                        actHolder.find("ul").remove();
                        actHolder.find(".placeholder").show(100, function () {
                            $(this).find("button").addClass("activated animated flash");
                        });
                    }
                }
            }
            else {
                var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
                var title = "<i class='fe-comment'></i> Something went wrong";
                displayDialogMsg(msg, title);
            }
            $('.mat-const-main .const-holder ul').perfectScrollbar("update");
        });
    }
    else {
        var index = -1;
        for (var i = 0; i < actMaterialQtyArry.length; i++) {
            if (actMaterialQtyArry[i].actID == actID && actMaterialQtyArry[i].loc == loc && actMaterialQtyArry[i].date == date && actMaterialQtyArry[i].matID == matID) {
                index = i;
                break
            }
        }
        if(index != -1)
            fruits.splice(index, 1);
    }
}
function updateMetarial() {
    $(".mat-const-main .const-holder.const-mats ul li").each(function () {
        var self = $(this);
        var actID = $("#matConstructor #actIdHidden").val();
        var loc = $("#matConstructor #locHidden").val();
        var date = $("#matConstructor #dateHidden").val();        
        var matID = self.find("select").val();
        var matQty = self.find(".act-amt").val();
        if (!matID.isEmpty() && !matQty.isEmpty()) {
            var isExist = false;
            for (var i = 0; i < actMaterialQtyArry.length; i++) {
                if (actMaterialQtyArry[i].actID == actID && actMaterialQtyArry[i].loc == loc && actMaterialQtyArry[i].date == date && actMaterialQtyArry[i].matID == matID) {
                    actMaterialQtyArry[i].matQty = matQty;
                    isExist = true;                    
                }
            }
            if (!isExist) {
                actMaterialQtyArry.push({
                    RID: self.data("id"),
                    actID: actID,
                    loc: loc,
                    date: date,
                    matID: matID,
                    matQty: matQty
                });
            }
        }
    });
    isMatAddedToist = true;
    //var wID = $("#matConstructor").data("id");
    //var invID = $("#matConstructor").data("inv");
    //var mats = [];
    //$(".mat-const-main .const-holder.const-mats ul li").each(function () {
    //    var self = $(this);
    //    var actID = self.find("select").val();
    //    var actQty = self.find(".act-amt").val();
    //    mats.push({
    //        RecID: self.data("id"),
    //        RID: actID,
    //        Amount: actQty
    //    });
    //});
    //var data = [];
    //data.push({
    //    WID: wID,
    //    InvId: invID,
    //    Activities: mats
    //});
    //$.post("/workorder/updateMetarials", $.toDictionary(data), function (data) {
    //    if (data.Code == "100") {
    //        ReLoadMetarias(wID);
    //        var msg = "<p>Metarials were <b>Updated</b> Successfully!.</p>";
    //        var title = "<i class='fe-comment'></i> Success";
    //        //displayDialogMsg(msg, title);
    //    }
    //    else {
    //        var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
    //        var title = "<i class='fe-comment'></i> Something went wrong";
    //        displayDialogMsg(msg, title);
    //    }
    //});
    closeMatConstructor();
}
function ReLoadMetarias(wid) {
    $.ajax({
        url: "/workorder/GenerateMetarialConstructor",
        data: { wid: wid },
        type: "POST",
        success: function (html) {
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#matConstructor [title]").tooltip({ placement: "top" });
                $("#matConstructor").addClass("animated bounceInRight").show();
                $('.mat-const-main .const-holder ul').perfectScrollbar();
                $("#matConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                $("#matConstructor .title span").dotdotdot();
                var actHolder = $(".mat-const-main .const-holder.const-mats");
                if (actHolder.find("li").length == 0) {
                    actHolder.find("ul").remove();
                    $(".mat-const-main .const-mats .placeholder").show(100, function () {
                        $(".mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                    });
                }
                else {
                    var ph = actHolder.find(".placeholder");
                    var time = ph.is(":visible") ? 300 : 10;
                    ph.find("button").removeClass("activated animated flash");
                    ph.hide(200);
                    setTimeout(function () {
                        setDisableMetarialOptions();
                        actHolder.find("select").select2({ width: "resolve" });
                        actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                        $('.mat-const-main .const-mats ul').perfectScrollbar();
                    }, time);
                }
            });
        }
    });
}
function closeMatConstructor() {
    $("#matConstructor").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
        $(this).remove();
        $(".full-overlay").fadeOut(200, function () { $(this).remove(); });
        if (!$("#con-wo").is(":visible") || $(".tech-tbl").length > 0) {
            //var page = dTable.page();
            //dTable.page(page).draw(false);
        }
        else {
            //var page = aDtable.page();
            //aDtable.page(page).draw(false);
            $.get("contractor/refreshConStats", { id: $("#sub-head").data("id") }, function (html) {
                $("#sub-head").replaceWith(html);
            });
        }
    });
}
/*End Meterial Constructor*/

/*Quotation Constructor */
function discardQuotationDataCheck(type) {
    if (wochanged)
        return true;
    switch (type) {
        case "full":
            return $("#qConstructor .const-holder ul li[class*='new-']").length > 0;
            break;
        case "acts":
            return $("#qConstructor .const-locs ul li.new-loc, #qConstructor .const-mats ul li.new-act").length > 0;
            break;
        case "locs":
            return $("#qConstructor .const-mats ul li.new-act").length > 0;
            break;
    }
}
function setDisableQuotationActivityOptions() {
    actsSelectedValue = [];
    var selects = $("#qConstructor .const-mats .act-holder select.act-ddl");
    selects.find("option.elsewhere").removeAttr("disabled");
    selects.each(function () {
        var val = $(this).val();
        actsSelectedValue.push(val);
    });
    for (var x = 0; x < actsSelectedValue.length; x++) {
        var val = actsSelectedValue[x];
        selects.each(function () {
            var self = $(this);
            var option = self.find("option[value='" + val + "']");
            if (self.val() != val) {
                option.addClass("elsewhere").attr("disabled", "disabled");
            }
            else {
                option.removeClass("elsewhere")
            }
        });
    }
}
function deleteQuotationActivity(type) {
    var val = temp.data("id");
    $.post("/quotation/deleteactivityfromquotation", { val: val }, function (data) {
        if (data.Code == "100") {
            temp.remove();
            if (type == "Activity") {
                var actHolder = $("#qConstructor .mat-const-main .const-holder.const-mats");
                if (actHolder.find("li").length < 1) {
                    actHolder.find("ul").remove();
                    actHolder.find(".placeholder").show(100, function () {
                        $(this).find("button").addClass("activated animated flash");
                    });
                }
            }
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
        $('#qConstructor .mat-const-main .const-holder ul').perfectScrollbar("update");
    });
}
function updateQuotationActivity() {
    var qID = $("#qConstructor").data("id");
    var acts = [];
    $("#qConstructor .mat-const-main .const-holder.const-mats ul li").each(function () {
        var self = $(this);
        var actID = self.find("select").val();
        var actQty = self.find(".act-amt").val();
        var actRate = self.find(".act-rat").val();
        acts.push({
            RecID: self.data("id"),
            ActDescr: actID,
            Qty: actQty,
            Rate: actRate
        });
    });
    var data = [];
    data.push({
        QID: qID,
        Activities: acts
    });
    $.post("/quotation/updateActivities", $.toDictionary(data), function (data) {
        if (data.Code == "100") {
            closeQuotConstructor();
            //ReLoadQuotationActivities(qID);
            var msg = "<p>Activities were <b>Updated</b> Successfully!.</p>";
            var title = "<i class='fe-comment'></i> Success";
            //displayDialogMsg(msg, title);
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
    });    
}
function ReLoadQuotationActivities(qid) {
    $.ajax({
        url: "/quotation/GenerateConstructor",
        data: { qid: qid },
        type: "POST",
        success: function (html) {
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#qConstructor [title]").tooltip({ placement: "top" });
                $("#qConstructor").addClass("animated bounceInRight").show();
                $('#qConstructor .mat-const-main .const-holder ul').perfectScrollbar();
                $("#qConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                $("#qConstructor .title span").dotdotdot();
                var actHolder = $(".mat-const-main .const-holder.const-mats");
                if (actHolder.find("li").length == 0) {
                    actHolder.find("ul").remove();
                    $("#qConstructor .mat-const-main .const-mats .placeholder").show(100, function () {
                        $("#qConstructor .mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                    });
                }
                else {
                    var ph = actHolder.find(".placeholder");
                    var time = ph.is(":visible") ? 300 : 10;
                    ph.find("button").removeClass("activated animated flash");
                    ph.hide(200);
                    setTimeout(function () {
                        setDisableQuotationActivityOptions();
                        actHolder.find("select").select2({ width: "resolve" });
                        actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                        $('#qConstructor .mat-const-main .const-mats ul').perfectScrollbar();
                    }, time);
                }
            });
        }
    });
}
function closeQuotConstructor() {
    $("#qConstructor").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
        $(this).remove();
        $(".full-overlay").fadeOut(200, function () { $(this).remove(); });
        if (!$("#con-wo").is(":visible") || $(".tech-tbl").length > 0) {
            var page = dTable.page();
            dTable.page(page).draw(false);
        }
        else {
            var page = aDtable.page();
            aDtable.page(page).draw(false);
            $.get("contractor/refreshConStats", { id: $("#sub-head").data("id") }, function (html) {
                $("#sub-head").replaceWith(html);
            });
        }
    });
}
/*End Quotation Constructor*/


/*Standby Invoice Constructor */
function discardSInvoiceDataCheck(type) {
    if (wochanged)
        return true;
    switch (type) {
        case "full":
            return $("#sIConstructor .const-holder ul li[class*='new-']").length > 0;
            break;
        case "acts":
            return $("#sIConstructor .const-locs ul li.new-loc, #sIConstructor .const-mats ul li.new-act").length > 0;
            break;
        case "locs":
            return $("#sIConstructor .const-mats ul li.new-act").length > 0;
            break;
    }
}
function setDisableSInvoiceActivityOptions() {
    actsSelectedValue = [];
    var selects = $("#sIConstructor .const-mats .act-holder select.act-ddl");
    selects.find("option.elsewhere").removeAttr("disabled");
    selects.each(function () {
        var val = $(this).val();
        actsSelectedValue.push(val);
    });
    for (var x = 0; x < actsSelectedValue.length; x++) {
        var val = actsSelectedValue[x];
        selects.each(function () {
            var self = $(this);
            var option = self.find("option[value='" + val + "']");
            if (self.val() != val) {
                option.addClass("elsewhere").attr("disabled", "disabled");
            }
            else {
                option.removeClass("elsewhere")
            }
        });
    }
}
function deleteSInvoiceActivity(type) {
    var val = temp.data("id");
    $.post("/StandbyInvoice/DeleteActivityFromStandbyInvoice", { val: val }, function (data) {
        if (data.Code == "100") {
            temp.remove();
            if (type == "Activity") {
                var actHolder = $("#sIConstructor .mat-const-main .const-holder.const-mats");
                if (actHolder.find("li").length < 1) {
                    actHolder.find("ul").remove();
                    actHolder.find(".placeholder").show(100, function () {
                        $(this).find("button").addClass("activated animated flash");
                    });
                }
            }
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
        $('#sIConstructor .mat-const-main .const-holder ul').perfectScrollbar("update");
    });
}
function updateSInvoiceActivity() {
    var sIID = $("#sIConstructor").data("id");
    var acts = [];
    $("#sIConstructor .mat-const-main .const-holder.const-mats ul li").each(function () {
        var self = $(this);
        var actID = self.find("select").val();
        var actQty = self.find(".act-amt").val();
        var actRate = self.find(".act-rat").val();
        acts.push({
            RecID: self.data("id"),
            ActId: actID,
            Qty: actQty,
            Rate: actRate
        });
    });
    var data = [];
    data.push({
        SIID: sIID,
        Activities: acts
    });
    $.post("/StandbyInvoice/updateActivities", $.toDictionary(data), function (data) {
        if (data.Code == "100") {
            closeSInvoiceConstructor();
            //ReLoadSInvoiceActivities(sIID);
            var msg = "<p>Activities were <b>Updated</b> Successfully!.</p>";
            var title = "<i class='fe-comment'></i> Success";
            //displayDialogMsg(msg, title);
        }
        else {
            var msg = "<p>Something went wrong while trying to complete your request. Please try again or contact your System Administrator.</p>";
            var title = "<i class='fe-comment'></i> Something went wrong";
            displayDialogMsg(msg, title);
        }
    });
}
function ReLoadSInvoiceActivities(sIid) {
    $.ajax({
        url: "/StandbyInvoice/GenerateConstructor",
        data: { sIid: sIid },
        type: "POST",
        success: function (html) {
            removeLoader(false, 500, function () {
                $("body").append(html);
                $("#sIConstructor [title]").tooltip({ placement: "top" });
                $("#sIConstructor").addClass("animated bounceInRight").show();
                $('#sIConstructor .mat-const-main .const-holder ul').perfectScrollbar();
                $("#sIConstructor .wo-inv select").select2({ minimumResultsForSearch: -1, width: 130, placeholder: "Select One", allowClear: true });
                $("#sIConstructor .title span").dotdotdot();
                var actHolder = $(".mat-const-main .const-holder.const-mats");
                if (actHolder.find("li").length == 0) {
                    actHolder.find("ul").remove();
                    $("#sIConstructor .mat-const-main .const-mats .placeholder").show(100, function () {
                        $("#sIConstructor .mat-const-main .const-mats .placeholder button").addClass("activated animated flash");
                    });
                }
                else {
                    var ph = actHolder.find(".placeholder");
                    var time = ph.is(":visible") ? 300 : 10;
                    ph.find("button").removeClass("activated animated flash");
                    ph.hide(200);
                    setTimeout(function () {
                        setDisableSInvoiceActivityOptions();
                        actHolder.find("select").select2({ width: "resolve" });
                        actHolder.find(".act-holder input.act-amt").onlyNumbers({ neagtive: false, decimal: true });
                        $('#sIConstructor .mat-const-main .const-mats ul').perfectScrollbar();
                    }, time);
                }
            });
        }
    });
}
function closeSInvoiceConstructor() {
    $("#sIConstructor").removeClass("fadeInRight").addClass("fadeOutUp").one('webkitAnimationEnd mozAnimationEnd MSAnimationEnd oanimationend animationend', function () {
        $(this).remove();
        $(".full-overlay").fadeOut(200, function () { $(this).remove(); });
        if (!$("#con-wo").is(":visible") || $(".tech-tbl").length > 0) {
            var page = dTable.page();
            dTable.page(page).draw(false);
        }
        else {
            var page = aDtable.page();
            aDtable.page(page).draw(false);
            $.get("contractor/refreshConStats", { id: $("#sub-head").data("id") }, function (html) {
                $("#sub-head").replaceWith(html);
            });
        }
    });
}
/*End Standby Invoice Constructor*/

/// <reference path="jquery-2.1.1.min.js" />
$(window).load(function () {
    var selectedCons = [];
    // menu events
    $("#nav > li.has-sub > a").click(function (e) {
        if ($("body").hasClass("big")) {
            var self = $(this);
            $("#nav > li.open").not(self.parents("li")).find("> a").trigger("click");
            self.parent().toggleClass("open");
            self.parent().find("ul").toggle(200);
            self.find("i:nth-child(2)").toggleClass("fe-right-open fe-down-open");
        }
        e.preventDefault();
    });
    $(document).on("mouseenter", ".sml #nav > li.has-sub", function () {
        $(this).find("ul").show(200);
    });
    $(document).on("mouseleave", ".sml #nav > li.has-sub", function () {
        $(this).find("ul").stop(true, true).hide(200);
    });
    $(document).mouseup(function (e) {
        var container = $(".user-holder");
        if (!container.is(e.target) && container.has(e.target).length === 0) {
            if ($(".user-dropdown").is(".active"))
                $(".user-dropdown").trigger("click");
        }
    });
    $("#nav li a").each(function () {
        var self = $(this);
        if (self.attr("href") == window.location.pathname) {
            self.addClass("active");
            if (self.parents("li.has-sub").length > 0)
                self.parents("li.has-sub").addClass("active").find("> a").trigger("click");
        }
    });

    $('.menu-toggler a').tooltip({ placement: "right" })
    $(document).on("click", '.menu-toggler a, button.navicon-button', function (e) {
        $(this).toggleClass("open")
        var sidebar = $("#sidebar");
        var subHolder = $(".sub-holder");
        $("body").toggleClass("sml big");
        if (!$("body").hasClass("sml")) {
            sidebar.find("a.active").parents(".has-sub").find("> a").trigger("click");
            var windowWidth = window.innerWidth;
            if (windowWidth <= 768) {
                $("#nav").hide();
                sidebar[0].style.height = '60px';
            }
            else {                
                sidebar.animate({ width: 240 }, 200, function () {
                    sidebar.find("li > a > span, li > a > .nav-arrow").show();
                    $("#logo h1").show();
                });
                subHolder.animate({ left: 240 }, 200);
                $("#header").animate({ left: 240 }, 200);
                //.animate({ left: 250 }, 200);
                $(".listing-selected").animate({ left: 240 }, 200);
            }
        }
        else {
            $("#logo h1").hide();
            sidebar.find("li > a > span, li > a > .nav-arrow").hide()
            sidebar.find(".has-sub.open > a").trigger("click").addClass("active");
            var windowWidth = window.innerWidth;
            if (windowWidth <= 768) {
                $("#nav").show();
                sidebar[0].style.height = '100%';
            }
            else {
                $("#logo h1").hide();
                sidebar.find("li > a > span, li > a > .nav-arrow").hide()                
                sidebar.animate({ width: 75 }, 200);
                subHolder.animate({ left: 75 }, 200);
                $(".listing-selected, #header").animate({ left: 75 }, 200);
                //$(".overlay").addClass("big");//.animate({ left: 85 }, 200);
            }
        }        
        e.preventDefault();
    });

    //sub menu events and actions
    $(document).on("click", "#nav-settings", function (e) {
        e.preventDefault();
        var url = $(this).data("url");
        $.post(url, function (html) {
            $("body").append(html);
            $('.modal').modal({ show: true }).find("select").select2({ minimumResultsForSearch: -1, width: "resolve" });
            $.validator.unobtrusive.parse($("#settings-box form"));
        });
    });
    $('#subnav li a').tooltip({ placement: "bottom" });

    $(document).on('click', '.user-dropdown', function (e) {
        var self = $(this);
        self.toggleClass("active");
        self.parent().find("ul").toggle();
        e.preventDefault();
    });

    //modal stuff
    $(document).on('hidden.bs.modal', '.modal', function(e) {
        $(this).remove();
    });

    //datatables stuff
    $(document).on("change", "#status", function () {
        dTable.draw();
    });

    $(document).on("click", ".admin-tbl tbody td.more-details", function () {
        var self = $(this);
        var tr = self.parent();
        var row = dTable.row(tr);
        self.toggleClass("expanded");
        self.find("span i").toggleClass("fe-plus fe-minus");
        if (row.child.isShown()) {
            row.child.hide();
        }
        else {
            row.child(row.data().Additional, "sub-row animated fadeIn").show();
        }
    });

    //misc
    $(document).on('click', function (e) {
        if (!$(e.target).hasClass("dt-change") && !$(e.target).parents("a").hasClass("dt-change")) {
            $(".dt-change").popover('hide');
        }
        if (!$(e.target).hasClass("dt-print") && !$(e.target).parents("a").hasClass("dt-print")) {
            $(".dt-print").popover('hide');
        }
        if (!$(e.target).hasClass("dt-print-new") && !$(e.target).parents("a").hasClass("dt-print-new")) {
            $(".dt-print-new").popover('hide');
        }
        return;
    });
    $('#body').mutate('height', function (element, info) {
        $('.sub-holder').perfectScrollbar('update');
    });
    $('.sub-list-holder').mutate('height', function (element, info) {
        $('.sub-list-holder').perfectScrollbar('update');
    });
    $(window).resize(function () {
        $('.ps-container').perfectScrollbar('update');
    });
    $('.sub-holder').perfectScrollbar();

    $(document).on("click", ".tel-form .tel-tabs button:not(:disabled)", function (e) {
        var self = $(this);
        if (self.is(":not(.active)")) {
            var btns = $(".tel-form .tel-tabs button");
            btns.removeClass("active");
            self.addClass("active");
            $(".modal-footer .update").removeClass("team-btn link-btn");
            //$(".modal-footer .update").toggleClass("saver team-btn");
            var index = btns.index(self);
            $(".tel-form table.active").removeClass("active").hide(200, function () {
                $(".tel-form table").eq(index).addClass("active").show(200);
                if (self.is("#teams")) {
                    setupTeamActions(true);
                    $(".modal-footer .update").toggleClass("saver team-btn");
                }
            });
        }
        e.preventDefault();
    });
    $(document).on('change', '.tel-form #banksddl', function (e) {
        var bid = $(this).val();
        var url = $(".admin-tbl").data("url") + "/getbranchesforbank";
        $.get(url, { id: bid }, function (branches) {
            $("#Branch").select2("destroy");
            var html = "<option></option>";
            for (var x = 0; x < branches.length; x++) {
                html += "<option value='" + branches[x].Value + "'>" + branches[x].Text + "</option>";
            }
            $("#Branch").html(html).select2({ minimumResultsForSearch: -1, width: "resolve", placeholder: "Select an option" });
        });
    });
    $(document).on("click", ".tel-form .tel-date > button", function (e) {
        $('.tel-form .tel-date .date-box').datepicker("show");
        $('.tel-form .tel-date .date-box').datepicker().on('changeDate', function (ev) {
            $(this).datepicker('hide');
        });
        e.preventDefault();
    });
    $(document).on("click", ".tel-form .tel-img-uploader", function (e) {
        $(this).parent().find("input[type='file']").trigger("click");
        e.preventDefault();
    })
    $(document).on("change", ".tel-form input[type='file']", function () {
        if (this.files && this.files[0]) {
            var reader = new FileReader();
            reader.onload = function (e) {
                $('.tel-form .pic-box img').attr('src', e.target.result);
            }
            reader.readAsDataURL(this.files[0]);
        }
    });
});