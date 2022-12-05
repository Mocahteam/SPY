mergeInto(LibraryManager.library, {

    Save: function (text) {
		var element = document.createElement('a');
		element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(Pointer_stringify(text)));
		element.setAttribute('download', 'NouveauScenario.xml');

		element.style.display = 'none';
		document.body.appendChild(element);

		element.click();

		document.body.removeChild(element);
	},
	
	ShowHtmlButtons: function () {
		var element = document.getElementById("proxyLoadButton");
		element.style.visibility = 'visible';
	},
	
	HideHtmlButtons: function () {
		var element = document.getElementById("proxyLoadButton");
		element.style.visibility = 'hidden';
	},
  
	IsMobileBrowser: function () {
		return (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent));
	}
});