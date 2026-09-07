<?php
class clsKlanten extends clsTableDef
{     
     public function __construct() 
     {
          parent::__construct();
          
          $column = new clsColumn();
          $column->setFieldName("klantnaam");
          $column->setCaption("Naam");         
          $this->columnHeader->addColumn($column);

          $column = new clsColumn();
          $column->setFieldName("telefoon");
          $column->setCaption("Telefoon");         
          $column->setEditType("Telephone");
          $this->columnHeader->addColumn($column);

          $column = new clsColumn();
          $column->setFieldName("email");
          $column->setCaption("Email");         
          $column->setEditType("Email");
          $this->columnHeader->addColumn($column);
          
          $this->selectsql = "SELECT * FROM klant ORDER BY klantnaam"; 
          $this->tablename = "klant";
          $this->key = "klant_id";
     }
}
     
class clsKlantSelect extends clsSelectColumn
{
     public function addNewSelect()
     {    $klanten = new clsKlanten; 
          
          return
              '<a href="#cps_klant_klant_id" data-toggle="collapse">
                    <i class="fa fa-plus"></i>
               </a>
               <div id="cps_klant_klant_id" class="col-xs-11 col-xs-offset-1 collapse"> 
                    <label>Nieuwe klant:</label>' .
                        $klanten->makeEditControls() .
                   '<input type="hidden" id="add_klant" name="add_klant" value="klant" />
               </div>';      
     }
}

?>
