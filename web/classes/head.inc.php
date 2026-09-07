<?php
	
class clsHead 
{   
     public function getHtml() 
     {
          $output = '
               <meta charset="UTF-8" />
               <meta http-equiv="X-UA-Compatible" content="IE=edge">
               <meta name="viewport" content="width=device-width, initial-scale=1.0">
               <title>Restaurant Excellent Taste</title>
               <meta name="keywords" content="Restaurant, Excellent taste, reserveren">
               <meta name="description" content="Excellent taste is een goed restaurant">
               <meta name="subject" content="Restaurant Excellent taste">
               <meta name="copyright" content="NewApps">
               <meta name="language" content="NL">
               <meta name="robots" content="index,follow">
               <meta name="author" content="Erik Steens, e.steens@rocgilde.nl">
               <meta name="designer" content="Erik Steens">
               <meta name="owner" content="Stichting Praktijkleren, Amersfoort">
               <meta name="pagename" content="Stichting Praktijkleren, Excellent Taste">
               <meta name="rating" content="General">
               <meta name="revisit-after" content="7 days">
               <meta name="target" content="all">
               <meta http-equiv="Expires" content="0">
               <meta http-equiv="Pragma" content="no-cache">
               <meta http-equiv="Cache-Control" content="no-cache">
               <meta http-equiv="imagetoolbar" content="no">
               <meta http-equiv="x-dns-prefetch-control" content="off">
               
               <link rel="stylesheet" href="' . CSS_PATH . 'bootstrap.min.css" />
               <link rel="stylesheet" href="' . CSS_PATH . 'font-awesome/css/font-awesome.min.css" />

               
               <link 	href="' . CSS_PATH . 'style.css" 
                         type="text/css" 
                         rel="stylesheet"/>
               
               <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js"></script>
               <script src="' . JS_PATH . 'bootstrap.min.js"></script>
               
               <script src="' . JS_PATH . 'functions.js"></script>
          ';
     
          return $output;
     }
     
     
}

