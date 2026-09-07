<?php
	session_start();
	error_reporting(E_ALL);
	ini_set("display_errors", "on");
	
	// Italian national format with 2 decimals`
	setlocale(LC_ALL, 'nl-NL');
	//us as: money_format('%.2n', $number) . "\n";

	
	define("ROOT_PATH"		, "");
	define("CSS_PATH"		, ROOT_PATH . "css/");
	define("JS_PATH"		, ROOT_PATH . "js/");
	define("IMAGES_PATH"	, ROOT_PATH . "images/");
	define("CLASSES_PATH"	, ROOT_PATH . "classes/");
	define("CONFIG_PATH"	, ROOT_PATH . "config/");
?>