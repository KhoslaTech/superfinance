/*
* Copyright (C) Khosla Tech Private Limited. All rights reserved.
* Part of ASP Security Kit (https://ASPSecurityKit.net )
* Author: Varun Om Khosla 
* This plugin extends jQuery.validate.unobtrusive.js to show errors as popup.
* In addition, it can show success (info)/error messages generated on the server in the popup as page loads.
* The errorProvider provides an API to programmatically display success/error messages as popup (jQuery dialog) from javascript code.
*/

(function ($) {
	var data_validation = "unobtrusiveValidation";

	errorProvider = window.errorProvider = {
		showErrorsAsPopup: true,

		showInfo: function (message, title) {
			this._show("message-success", message, title);
		},

		showError: function (message, title) {
			this._show("validation-summary-errors", message, title);
		},

		showExisting: function () {
			// This is used to show the message populated in the errorProvider from the server. Based on the class – success or error – it determines the appropriate method to be called.
			this._insureErrorDialog();

			if (this._$errorDialogDiv.hasClass("validation-summary-errors")) {
				this.showError(this._$errorDialogDiv.html(), "Error");
			}
			else if (this._$errorDialogDiv.hasClass("message-success")) {
				this.showInfo(this._$errorDialogDiv.html(), "Message");
			}
		},

		// private members
		_$errorDialogDiv: null,
		_show: function (cssClass, message, title) {
			this._insureErrorDialog();
			this._clearCss();

			this._$errorDialogDiv.addClass(cssClass)
				.html('') // Clear before appending.
				.append(message) // append supports both html strings as well as jquery/dom elements
				.dialog('open')
				.dialog('option', 'title', title);
		},

		_clearCss: function () {
			this._$errorDialogDiv.removeClass("validation-summary-errors").removeClass("validation-summary-valid").removeClass("message-success");
		},

		_insureErrorDialog: function () {
			if (this._$errorDialogDiv != null)
				return;

			var self = this;

			var $errorDlg = $("#errorProvider");
			if ($errorDlg.length == 0) {
				$errorDlg = $("<div id='errorProvider'></div>").appendTo(document.body);
			}

			$errorDlg.dialog({
				autoOpen: false,
				width: 'auto',
				show: 'fade',
				hide: 'fade',
				modal: true,
				title: 'Alert',
				buttons: [{
					text: 'Close',
					click: function () {
						self._$errorDialogDiv.dialog('close');
					}
				}]
			});

			this._$errorDialogDiv = $errorDlg;
		}
	};

	function showError(event, validator) {
		if (validator.errorList.length) {
			var list = $("<ol></ol>").addClass("error-list");

			$.each(validator.errorList, function () {
				if (!this.element) {
					$("<li />").html(this.message).appendTo(list);
				}
			});
			if (list[0].childElementCount > 0) {
				errorProvider.showError(list);
			}
		}
	}

	function onErrors(event, validator) {
		if (errorProvider.showErrorsAsPopup) {
			showError(event, validator);
			return;
		}

		var container = $(document).find("[data-valmsg-summary=true]"),
			list = container.find("ul");

		if ((!list || !list.length) && validator.errorList.length) {
			container.empty();
			list = $("<ul/>").appendTo(container);
		}

		if (list && list.length && validator.errorList.length) {
			list.empty();
			container.addClass("validation-summary-errors").removeClass("validation-summary-valid").removeClass("message-success");

			$.each(validator.errorList, function () {
				$("<li />").html(this.message).appendTo(list);
			});
		}
	}

	function onReset(event) {
		$(document).find(".validation-summary-errors")
			.addClass("validation-summary-valid")
			.removeClass("validation-summary-errors");
	}

	$(function () {
		if (errorProvider.showErrorsAsPopup) {
			errorProvider.showExisting();
		}

		$("form").each(function () {
			var $form = $(this),
				result = $form.data(data_validation);

			if (result) {
				$form
					.unbind("invalid-form.validate", result.options.invalidHandler) // Removing default unobtrusive handler.
					.bind("invalid-form.validate", onErrors)
					.unbind("reset." + data_validation, onReset)
					.bind("reset." + data_validation, onReset);
			}
		});
	});
}(jQuery));
