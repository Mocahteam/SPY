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
		var element = document.getElementById("proxyLoadFiles");
		element.style.visibility = 'visible';
	},
	
	HideHtmlButtons: function () {
		var element = document.getElementById("proxyLoadFiles");
		element.style.visibility = 'hidden';
	},
  
	IsMobileBrowser: function () {
		return (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent));
	},
	
    DownloadLevel: function (uri) {
		console.log(Pointer_stringify(uri));
		var element = document.createElement('a');
		element.setAttribute('href', Pointer_stringify(uri));
		element.setAttribute('target', "_blank");

		element.style.display = 'none';
		document.body.appendChild(element);

		element.click();

		document.body.removeChild(element);
	},

	GetBrowserLanguage: function(){
		if ((navigator.language && navigator.language.includes("fr")) || (navigator.userLanguage && navigator.userLanguage.includes("fr")))
			return "fr";
		else
			return "en";
	}
});