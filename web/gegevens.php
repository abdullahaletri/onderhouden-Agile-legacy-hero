<?php
	require_once("includes/includes.php");
     require_once(CLASSES_PATH . "table.inc.php");
     require_once(CLASSES_PATH . "klanten.inc.php");	
	
class clsDrinkenEnEten extends clsTableDef
{     
     public function __construct() 
     {
          parent::__construct();
          
          $this->selectsql = ""; // is set in derived class
          $this->tablename = "menuitem";
          $this->key = "menuitemcode";
          
          $column = new clsColumn();
          $column->setFieldName("menuitemcode");
          $column->setCaption("Code");         
          $this->columnHeader->addColumn($column);
          
          $column = new clsColumn();
          $column->setFieldName("menuitemnaam");
          $column->setCaption("Omschrijving");         
          $this->columnHeader->addColumn($column);
          
          $column = new clsColumn();
          $column->setFieldName("prijs");
          $column->setCaption("Prijs");
          $this->columnHeader->addColumn($column);
          
          $column = new clsSelectColumn();
          $column->setFieldName("subgerechtcode");
          $column->setCaption("Valt onder");
          $column->setEditType("Select");
          $column->setLookUpSql("SELECT CONCAT(s.subgerechtcode, ' - ', s.subgerechtnaam, '->', g.gerechtnaam) as lookupresult,
                                        s.subgerechtcode as lookup_id
                                   FROM subgerecht s 
                                   LEFT JOIN gerecht g 
                                          ON s.gerechtcode = g.gerechtcode");
          $this->columnHeader->addColumn($column);         
     }  
}

class clsDrinken extends clsDrinkenEnEten
{
     public function __construct() 
     {
          parent::__construct();

          $this->selectsql = "SELECT m.menuitemnaam, m.menuitemcode, m.prijs, m.subgerechtcode    
                                FROM menuitem m 
                                LEFT JOIN subgerecht s 
                                       ON m.subgerechtcode = s.subgerechtcode
                              WHERE s.gerechtcode = 'drk'";
     }      
}

class clsEten extends clsDrinkenEnEten
{     
     public function __construct() 
     {
          parent::__construct();

          $this->selectsql = "SELECT m.menuitemnaam, m.menuitemcode, m.prijs, m.subgerechtcode  
                                FROM menuitem m 
                                LEFT JOIN subgerecht s 
                                       ON m.subgerechtcode = s.subgerechtcode
                              WHERE s.gerechtcode <> 'drk'";
     }      
}
	
class clsSubGerechten extends clsTableDef
{     
     public function __construct() 
     {
          parent::__construct();
          
          $this->selectsql = "SELECT subgerechtcode, subgerechtnaam, gerechtcode
                                FROM subgerecht
                              ORDER BY subgerechtnaam";
          $this->tablename = "subgerecht";
          $this->key = "subgerechtcode";
          
          $column = new clsColumn();
          $column->setFieldName("subgerechtcode");
          $column->setCaption("Code");
          $column->setReadOnly();
          $this->columnHeader->addColumn($column);
          
          $column = new clsColumn();
          $column->setFieldName("subgerechtnaam");
          $column->setCaption("Omschrijving");         
          $this->columnHeader->addColumn($column);         
          
          $column = new clsSelectColumn();
          $column->setFieldName("gerechtcode");
          $column->setCaption("Valt onder");
          $column->setEditType("Select");
          $column->setLookUpSql("SELECT gerechtnaam as lookupresult, gerechtcode as lookup_id
                                   FROM gerecht 
                                 ORDER BY gerechtnaam");
          $this->columnHeader->addColumn($column);         
     }  
}

class clsGerechten extends clsTableDef
{     
     public function __construct() 
     {
          parent::__construct();
          
          $this->selectsql = "SELECT gerechtcode, gerechtnaam 
                                FROM gerecht
                              ORDER BY gerechtnaam"; 
          $this->tablename = "gerecht";
          $this->key = "gerechtcode";
          
          $column = new clsColumn();
          $column->setFieldName("gerechtcode");
          $column->setCaption("Code");
          $column->setReadOnly();
          $this->columnHeader->addColumn($column);
          
          $column = new clsColumn();
          $column->setFieldName("gerechtnaam");
          $column->setCaption("Omschrijving");         
          $this->columnHeader->addColumn($column);                   
     }  
}

class clsPage extends clsDefaultPage
{	
     protected function contentHtml() 
     {	
          $soort = "";		
          if (isset($_GET['soort'])) 
          {    $soort = $_GET['soort'];
               $keyvalue = false;		
               if (isset($_GET['key'])) 
               {    $keyvalue = $_GET['key'];
               }  
               $this->datalist = $this->getTableDef($soort, $keyvalue);
          }
          else
          {    return "Onbekend gegeven.";
          }
          return $this->getActionHtml();
                    
     } 
     
     protected function getTableDef($soort, $keyvalue) 
     {
          $datalist = false;
          switch($soort) 
          {
               case "drinken"     : $datalist = new clsDrinken(); break;
               case "eten"        : $datalist = new clsEten(); break;
               case "klanten"     : $datalist = new clsKlanten(); break;
               case "gerechten"   : $datalist = new clsGerechten(); break;
               case "subgerechten": $datalist = new clsSubgerechten(); break;
          }
          if (!$datalist)
          {
               print "Onbekend gegeven.";
               die;
          }        
          $datalist->setSoort($soort);
          $datalist->setKeyValue($keyvalue);
          return $datalist;
     }    
}

     $page = new clsPage();
	echo $page->getHtml();
?>