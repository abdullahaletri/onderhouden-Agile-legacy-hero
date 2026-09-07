<?php

require_once("includes/includes.php");
require_once(CLASSES_PATH . "table.inc.php");	
	
class clsOverzicht extends clsTableDef
{     
     public function __construct() 
     {
          parent::__construct();
                   
          $vandaag = date("Y-m-d");
          $this->selectsql = "SELECT b.tafel AS tafelnr, 
                                     b.aantal AS aantalitems, 
                                     m.menuitemnaam AS naam, 
                                     r.reservering_id AS res_id 
                                   FROM reservering r
                                   LEFT JOIN bestelling b 
                                     ON r.reservering_id = b.reservering_id
                                   LEFT JOIN menuitem m 
                                     ON b.menuitemcode = m.menuitemcode
                                   LEFT JOIN subgerecht s 
                                     ON m.subgerechtcode = s.subgerechtcode 
                                   WHERE r.datum = '$vandaag'  
                                     AND s.gerechtcode "; 
          $this->tablename = "reservering";
          $this->key = "res_id";
          $this->setReadOnly(true);
          
          $column = new clsColumn();
          $column->setFieldName("tafelnr");
          $column->setCaption("Tafel");         
          $this->columnHeader->addColumn($column);
          
          $column = new clsColumn();
          $column->setFieldName("aantalitems");
          $column->setCaption("Aantal");         
          $this->columnHeader->addColumn($column);
          
          $column = new clsColumn();
          $column->setFieldName("naam");
          $column->setCaption("Gerecht");
          $this->columnHeader->addColumn($column);
     } 
}

class clsOverzichtKok extends clsOverzicht
{
     public function getSelectSql()
     {// Overwrite
          return $this->selectsql .= "<> 'drk'"; 
     }
}

class clsOverzichtOber extends clsOverzicht
{     
     public function getSelectSql()
     { // Overwrite
          return $this->selectsql .= "= 'drk'"; 
     }
}

class clsPage extends clsDefaultPage
{
     protected function contentHtml()
     {	// Gewijzigd n.a.v. ticketnummer [[245]]		
          if (isset($_GET['overzicht'])) 
          {
               return $this->getOverzicht($_GET['overzicht']);
          }
          return "";
          // Einde gewijzigd n.a.v. ticketnummer [[245]]
     }
     
     protected function getOverzicht($voorWie) 
     {         
          if ($voorWie == "kok")
          {    $this->datalist = new clsOverzichtKok();
          }
          else
          {    $this->datalist = new clsOverzichtOber();
          }
          return $this->datalist->getTableHtml();
     }
}
     
     $page = new clsPage();
	echo $page->getHtml();
	
?>
