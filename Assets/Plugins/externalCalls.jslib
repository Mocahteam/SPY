mergeInto(LibraryManager.library, {

    Save: function (text, defaultName) {
		var element = document.createElement('a');
		element.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(Pointer_stringify(text)));
		element.setAttribute('download', Pointer_stringify(defaultName));

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
		var element = document.createElement('a');
		element.setAttribute('href', Pointer_stringify(uri));
		element.setAttribute('target', "_blank");

		element.style.display = 'none';
		document.body.appendChild(element);

		element.click();

		document.body.removeChild(element);
	},

	GetBrowserLanguage: function(){
		var ret = ""
		const params = new URLSearchParams(window.location.search);
		if (params.get("lang") != null){
			if (params.get("lang") == "fr")
				ret = "fr";
			else if (params.get("lang") == "en")
				ret = "en";
		}
		if (ret == ""){
			if ((navigator.language && navigator.language.includes("fr")) || (navigator.userLanguage && navigator.userLanguage.includes("fr")))
				ret = "fr";
			else
				ret = "en";
		}
		//Get size of the string
		var bufferSize = lengthBytesUTF8(ret) + 1;
		//Allocate memory space
		var buffer = _malloc(bufferSize);
		//Copy old data to the new one then return it
		stringToUTF8(ret, buffer, bufferSize);
		return buffer;
	},
	
	ClearPlayerPrefs: function(){
		const params = new URLSearchParams(window.location.search);
		return params.get("clear") != null;
	},

	UpdateHTMLLanguage: function(newLang){
		var lang = Pointer_stringify(newLang);
		document.updateLang(lang);
	}
});