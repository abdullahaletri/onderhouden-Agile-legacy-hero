<?php
abstract class clsDefaultPage 
{
     protected $datalist; 
     
     public function __construct() 
     {
          require_once(CONFIG_PATH . "database.inc.php");
          $this->connection = database::connect();
          $this->datalist = false;
     }
     
     private function headHtml()
     {
          require_once(CLASSES_PATH . "head.inc.php");
          $head = new clsHead;
          return $head->getHtml();
     }
     
     private function mainMenuHtml()
     {
          require_once(CLASSES_PATH . "mainmenu.inc.php");
          $mainMenu = new clsMainMenu;
          return $mainMenu->getHtml();
     }
     
     // deze procedute MOET in de afgeleide klasse worden geimplementeerd
     abstract protected function contentHtml(); 
     
     private function footerHtml()
     {
          require_once(CLASSES_PATH . "footer.inc.php");
          $footer = new footer;
          return $footer->getHtml();
     }
          
     protected function getActionHtml()
     {
          $action = "";		
          if (isset($_GET['action'])) 
          {    $action = $_GET['action'];
          }
          switch($action) 
          {
               case "edit": 
                    return $this->datalist->getEditHtml(); break;
               case "new": 
                    return $this->datalist->getNewHtml(); break;
               case "save": 
                    return $this->datalist->getUpdateHtml(); break;
               case "insert": 
                    return $this->datalist->getInsertHtml(); break;
               case "delete": 
                    return $this->datalist->getDeleteHtml(); break;
               default:   
                    return $this->datalist->getTableHtml(); break;
          } 
     }
     
     public function getHtml()
     { 
          $output = "";
          $output .= 
               '<!DOCTYPE html>
                     <html>
                          <head>'.
                          $this->headHtml();	
          $output .=	'</head>
                          <body lang="nl">
                               <nav class="navbar navbar-default">
                                    <div class="container-fluid">
                                         <header>
                                              <div id="topbar"></div>';
          $output .=                               $this->mainMenuHtml();
          $output .=	               '</header>
                                         <content> 
                                              <div class="row"> 
                                                   <div class="col-xs-12 col-sm-12 col-lg-12">';
          $output .=                                    $this->contentHtml();
          $output .=                              '</div>
                                              </div>
                                         </content>
                                         <footer>';
          $output .=                         '<hr />' .$this->footerHtml() .
                                        '</footer>
                                   </div>
                              </nav>
                          </body>
                     </html>';
          
          //return the entire requested page
          return $output;

     }
     
}
?>