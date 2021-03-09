/// <binding AfterBuild='all' />
module.exports = function (grunt) {
	grunt.initConfig({
		clean: [
			"wwwroot/css/dist/*",
			"wwwroot/css/images/*",
			"wwwroot/css/fonts/*",
			"wwwroot/scripts/dist/*",
			"wwwroot/temp/*"
		],
		concat: {
			jquery_validate: {
				src: ['node_modules/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.js', 'wwwroot/scripts/ask/jquery.validate.unobtrusive.plugin.js'],
				dest: 'wwwroot/temp/jquery.validate.unobtrusive.combined.js'
			},
			jtable: {
				src: ['node_modules/jtable/lib/jquery.jtable.js', 'wwwroot/scripts/ask/jquery.jtable.ask.js'],
				dest: 'wwwroot/temp/jquery.jtable.combined.js'
			},
			css: {
				src: ['wwwroot/css/bootswatch.css', 'node_modules/jquery-ui-dist/jquery-ui.css', 'wwwroot/css/site.css', 'node_modules/font-awesome/css/font-awesome.css'],
				dest: 'wwwroot/temp/site.combined.css'
			}
		},
		cssmin:
		{
			css:
			{
				src: ['wwwroot/temp/site.combined.css'],
				dest: 'wwwroot/css/dist/site.min.css'
			}
		},
		uglify: {
			jquery_validate: {
				src: ['wwwroot/temp/jquery.validate.unobtrusive.combined.js'],
				dest: 'wwwroot/scripts/dist/jquery.validate.unobtrusive.min.js'
			},
			jtable: {
				src: ['wwwroot/temp/jquery.jtable.combined.js'],
				dest: 'wwwroot/scripts/dist/jtable.min.js'
			},
			permission: {
				src: ['wwwroot/scripts/ask/permissions.js'],
				dest: 'wwwroot/scripts/dist/permissions.min.js'
			}
		},
		modernizr: {
			dist: {
				"parseFiles": true,
				"customTests": [],
				"dest": "wwwroot/scripts/dist/modernizr.min.js",
				"tests": [
					"ambientlight",
					"applicationcache",
					"audio",
					"batteryapi",
					"blobconstructor",
					"canvas",
					"canvastext",
					"contenteditable",
					"contextmenu",
					"cookies",
					"cors",
					"cryptography",
					"customelements",
					"customprotocolhandler",
					"customevent",
					"dart",
					"dataview",
					"emoji",
					"eventlistener",
					"exiforientation",
					"flash",
					"forcetouch",
					"fullscreen",
					"gamepads",
					"geolocation",
					"hashchange",
					"hiddenscroll",
					"history",
					"htmlimports",
					"ie8compat",
					"indexeddb",
					"indexeddbblob",
					"input",
					"search",
					"inputtypes",
					"intl",
					"json",
					"ligatures",
					"olreversed",
					"mathml",
					"MessageChannel",
					"notification",
					"pagevisibility",
					"performance",
					"pointerevents",
					"pointerlock",
					"postmessage",
					"proximity",
					"queryselector",
					"quotamanagement",
					"requestanimationframe",
					"serviceworker",
					"svg",
					"templatestrings",
					"touchevents",
					"typedarrays",
					"unicoderange",
					"unicode",
					"userdata",
					"vibrate",
					"video",
					"vml",
					"webintents",
					"animation",
					"webgl",
					"websockets",
					"xdomainrequest",
					"adownload",
					"audioloop",
					"audiopreload",
					"webaudio",
					"lowbattery",
					"canvasblending",
					[
						"todataurljpeg",
						"todataurlpng",
						"todataurlwebp"
					],
					[
						"canvaswinding"
					],
					"getrandomvalues",
					"cssall",
					"cssanimations",
					"appearance",
					"backdropfilter",
					"backgroundblendmode",
					"backgroundcliptext",
					"bgpositionshorthand",
					"bgpositionxy",
					[
						"bgrepeatspace",
						"bgrepeatround"
					],
					"backgroundsize",
					"bgsizecover",
					"borderimage",
					"borderradius",
					"boxshadow",
					"boxsizing",
					"csscalc",
					"checked",
					"csschunit",
					"csscolumns",
					[
						"cssgrid",
						"cssgridlegacy"
					],
					"cubicbezierrange",
					"display-runin",
					"displaytable",
					"ellipsis",
					"cssescape",
					"cssexunit",
					"cssfilters",
					"flexbox",
					"flexboxlegacy",
					"flexboxtweener",
					"flexwrap",
					"focuswithin",
					"fontface",
					"generatedcontent",
					"cssgradients",
					"hairline",
					"hsla",
					[
						"csshyphens",
						"softhyphens",
						"softhyphensfind"
					],
					"cssinvalid",
					"lastchild",
					"cssmask",
					"mediaqueries",
					"multiplebgs",
					"nthchild",
					"objectfit",
					"opacity",
					"overflowscrolling",
					"csspointerevents",
					"csspositionsticky",
					"csspseudoanimations",
					"csspseudotransitions",
					"cssreflections",
					"regions",
					"cssremunit",
					"cssresize",
					"rgba",
					"cssscrollbar",
					"scrollsnappoints",
					"shapes",
					"siblinggeneral",
					"subpixelfont",
					"supports",
					"target",
					"textalignlast",
					"textshadow",
					"csstransforms",
					"csstransforms3d",
					"csstransformslevel2",
					"preserve3d",
					"csstransitions",
					"userselect",
					"cssvalid",
					[
						"variablefonts"
					],
					"cssvhunit",
					"cssvmaxunit",
					"cssvminunit",
					"cssvwunit",
					"willchange",
					"wrapflow",
					"classlist",
					[
						"createelementattrs",
						"createelement-attrs"
					],
					"dataset",
					"documentfragment",
					"hidden",
					"microdata",
					"mutationobserver",
					"passiveeventlisteners",
					"bdi",
					"datalistelem",
					"details",
					"outputelem",
					"picture",
					[
						"progressbar",
						"meter"
					],
					"ruby",
					"template",
					"time",
					[
						"texttrackapi",
						"track"
					],
					"unknownelements",
					"es5array",
					"es5date",
					"es5function",
					"es5object",
					"es5",
					"strictmode",
					"es5string",
					"es5syntax",
					"es5undefined",
					"es6array",
					"arrow",
					"es6collections",
					"contains",
					"generators",
					"es6math",
					"es6number",
					"es6object",
					"promises",
					"es6string",
					[
						"devicemotion",
						"deviceorientation"
					],
					"oninput",
					"filereader",
					"filesystem",
					"capture",
					"fileinput",
					"directory",
					"formattribute",
					"localizednumber",
					"placeholder",
					"requestautocomplete",
					"formvalidation",
					"sandbox",
					"seamless",
					"srcdoc",
					"apng",
					"imgcrossorigin",
					"jpeg2000",
					"jpegxr",
					"sizes",
					"srcset",
					"webpalpha",
					"webpanimation",
					[
						"webplossless",
						"webp-lossless"
					],
					"webp",
					"inputformaction",
					"inputformenctype",
					"inputformmethod",
					"inputformtarget",
					"hovermq",
					"pointermq",
					"beacon",
					"lowbandwidth",
					"eventsource",
					"fetch",
					"xhrresponsetypearraybuffer",
					"xhrresponsetypeblob",
					"xhrresponsetypedocument",
					"xhrresponsetypejson",
					"xhrresponsetypetext",
					"xhrresponsetype",
					"xhr2",
					"scriptasync",
					"scriptdefer",
					"speechrecognition",
					"speechsynthesis",
					"localstorage",
					"sessionstorage",
					"websqldatabase",
					"stylescoped",
					"svgasimg",
					"svgclippaths",
					"svgfilters",
					"svgforeignobject",
					"inlinesvg",
					"smil",
					"textareamaxlength",
					"bloburls",
					"datauri",
					"urlparser",
					"urlsearchparams",
					"videoautoplay",
					"videocrossorigin",
					"videoloop",
					"videopreload",
					"webglextensions",
					"datachannel",
					"getusermedia",
					"peerconnection",
					"websocketsbinary",
					[
						"atobbtoa"
					],
					"framed",
					"matchmedia",
					"blobworkers",
					"dataworkers",
					"sharedworkers",
					"transferables",
					"webworkers"
				],
				"options": [
					"setClasses"
				],
				"uglify": true
			}
		},
		copy: {
			jquery_js: {
				src: 'node_modules/jquery/dist/jquery.min.js',
				dest: 'wwwroot/scripts/dist/jquery.min.js'
			},
			jquery_validate_js: {
				src: 'node_modules/jquery-validation/dist/jquery.validate.min.js',
				dest: 'wwwroot/scripts/dist/jquery.validate.min.js'
			},
			jquery_ui_js: {
				src: 'node_modules/jquery-ui-dist/jquery-ui.min.js',
				dest: 'wwwroot/scripts/dist/jquery-ui.min.js'
			},
			jquery_ui_images: {
				src: 'node_modules/jquery-ui-dist/images/*.png',
				dest: 'wwwroot/css/dist/images/',
				expand: true,
				flatten: true
			},
			jtable_themes: {
				cwd: 'node_modules/jtable/lib/themes',
				src: '**/*',
				dest: 'wwwroot/scripts/dist/jtable/themes',
				expand: true
			},
			fontawesome_fonts: {
				cwd: 'node_modules/font-awesome/fonts',
				src: '**/*',
				dest: 'wwwroot/css/fonts/',
				expand: true,
				flatten: true
			},
			bootstrap: {
				src: 'node_modules/bootstrap/dist/js/bootstrap.min.js',
				dest: 'wwwroot/scripts/dist/bootstrap.min.js'
			}
		}
	});

	grunt.loadNpmTasks("grunt-contrib-clean");
	grunt.loadNpmTasks("grunt-contrib-concat");
	grunt.loadNpmTasks("grunt-contrib-uglify");
	grunt.loadNpmTasks('grunt-contrib-watch');
	grunt.loadNpmTasks('grunt-contrib-cssmin');
	grunt.loadNpmTasks('grunt-contrib-copy');
	grunt.loadNpmTasks("grunt-modernizr");

	grunt.registerTask("all", ['clean', 'concat', 'uglify', 'cssmin', 'modernizr', 'copy']);
};