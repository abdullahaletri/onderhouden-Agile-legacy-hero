<?php

class clsColumnHeader
{
     private $columns;
     
     public function __construct() 
     {
          $this->columns = array();
     } 
     
     public function addColumn($column)
     {
          $this->columns[] = $column;
     }    
    
     public function getColumns()
     {
          return $this->columns;
     }    
     
}

class clsColumn
{
     private $fieldName;
     private $fieldCaption;
     private $edittype; /* Bootstrap input types: Text, Search, Email, URL, Telephone
                                                  Password, Number, Date and time, 
                                                  Date, Month, Week, Time*/
     private $readonly;
     private $columnhtml;
     
     public function __construct() 
     {
          $this->edittype = "Text";
          $this->readonly = false;
          $this->columnhtml = false;
          $this->lookupAdd = false;
     } 
     
     public function getCaption()
     {
          return $this->fieldCaption;
     }
     
     public function setCaption($caption)
     {
          $this->fieldCaption = $caption;
     }
     
     public function getFieldName()
     {
          return $this->fieldName;
     }
     public function setFieldName($fieldname)
     {
          $this->fieldName = $fieldname;
     }
     
     public function getEditType()
     {
          return $this->edittype;
     }
     
     public function setEditType($edittype)
     {
          $this->edittype = $edittype;
     }
     
     public function getLookUpSql()
     {
          return $this->lookupsql;
     }
     
     public function setLookUpSql($lusql)
     {
          $this->lookupsql = $lusql;
     }   
     
     public function getReadOnly()
     {
          return $this->readonly || $this->columnhtml;
     }
     
     public function setReadOnly($value = true)
     {
          $this->readonly = $value;
     }
     
     public function getColumnHtml()
     {
          return $this->columnhtml;
     }
     
     public function setColumnHtml($value)
     {
          $this->columnhtml = $value;
     }
     
     public function parseFieldValues($row)
     {    $output = $this->columnhtml;
          $start = stripos($output, "<!");
          while ($start > -1)
          {    $eind = stripos($output, "!>");
               $sub = substr($output, $start + 2, $eind - $start - 2);
               $output = str_replace("<!" . $sub . "!>", $row[$sub], $output);
               $start = stripos($output, "<!");
          }
          return $output;          
     }  
     
     public function getFieldValueFromDB($row)
     {
          return $row[$this->getFieldName()];
     }
     
     public function setFieldValueToDB($value)
     {
          return $value;
     }
}

class clsSelectColumn extends clsColumn
{
     private $lookupsql;

     public function __construct() 
     {
          $this->lookupsql = false;
     } 
     
     public function getLookUpSql()
     {
          return $this->lookupsql;
     }
     
     public function setLookUpSql($lusql)
     {
          $this->lookupsql = $lusql;
     } 
     
     public function addNewSelect()
     {
          return "";
     }     
}

class clsDateColumn extends clsColumn
{
     public function getFieldValueFromDB($row)
     {
          return $this->convertDateSQLFormat($row[$this->getFieldName()], 1);
     } 
     
     public function setFieldValueToDB($value)
     {
          return $this->convertDateSQLFormat($value, -1);
     }
     
     private function convertDateSQLFormat($datum, $toSQL) 
     {
          if ($toSQL > 0 ) {
            $date = new DateTime($datum);
            return $date->format('d-m-Y');
          }
          return $datum;
     }
     
}

class clsTableDef
{
     protected $connection;
     protected $header;
     protected $body;
     protected $tablename;
     protected $selectsql;
     protected $key;
     protected $keyvalue;
     protected $soort;
     protected $readonly;
     protected $title;
    
     public function __construct() 
     {
          $this->connection = database::connect();
          $this->columnHeader = new clsColumnHeader();
     } 
     
     public function getHeader()
     {
          return $this->columnHeader;
     }  
     
     public function getSelectSql()
     {
          return $this->selectsql;
     } 
     
     public function getKey()
     {
          return $this->key;
     } 

     public function getKeyValue()
     {
          return $this->keyvalue;
     } 

     public function setKeyValue($value)
     {
          $this->keyvalue = $value;
     } 

     public function getTableName()
     {
          return $this->tablename;
     } 

     public function setSoort($soort)
     {
          $this->soort = $soort;
     } 
     
     public function setReadOnly($value = false)
     {
          $this->readonly = $value;
     }
     
     public function setTitle($value)
     {
          $this->title = $value;
     } 
     
     private function makeSelect($value, $name, $column)
     {
          $output =  
               "<select class='selectpicker form-control' 
                        id='" . $name . "'  
                        name='" . $name . "'>";
                        
          foreach($this->connection->query($column->getLookUpSql()) as $row) 
          {    $output .= "
                     <option value = '" . $row['lookup_id'] . "' ";
               if ($row['lookup_id'] == $value)
               {    $output .= "
                             selected='selected' ";
               }
               $output .= "
                     <option>" . $row['lookupresult'] . "</option>";
          }
          $output .= 
               "</select>";
          return $output;
     }
     
     private function makeInput($value, $name, $column)
     {    $readonly = " readonly='readonly'";
          $readonlyclass = " form-control-plaintext"; // Gewijzigd n.a.v. ticketnummer [[229]]
          if (!$column->getReadOnly())
          {    $readonly = "";
               $readonlyclass = "";                  // Gewijzigd n.a.v. ticketnummer [[229]]
          }
          
          return "<input type='" . $column->getEditType() . "' 
                         class='form-control$readonlyclass'
                         $readonly 
                         value='" . $value . "'  
                         id='" . $name . "'  
                         name='" . $name . "'>";   // Gewijzigd n.a.v. ticketnummer [[229]]
     }
     
     private function makeFormControlSet($column, $row = false)
     {    $output = "";
          if (!$column->getReadOnly())
          {
               $fieldname = $column->getFieldName();
               if (!$row)
               {    $value = "";
               }
               else
               {    $value = $column->getFieldValueFromDB($row);
               }
               $name = $fieldname . "_id";
               $fieldcaption = $column->getCaption();
               $controlhtml = "";
               $collapse = "";
               $edittype = $column->getedittype();
               switch($edittype) 
               {
                    case "Text":
                    case "Search":
                    case "Email":
                    case "URL":
                    case "Telephone":
                    case "Password":
                    case "Number":
                    case "Date":
                    case "Date and time":
                    case "Month":
                    case "Week":
                    case "Time":                       // Gewijzigd n.a.v. ticketnummer [[228]]
                              $controlhtml = $this->makeInput($value, $name, $column); 
                              break;
                    case "Select": 
                              $controlhtml = $this->makeSelect($value, $name, $column); 
                              $collapse = $column->addNewSelect();
                              break;
               }
               
               
               $output = 
                   "<div class='form-group'>
                         <label for='" . $name . "'>" . $fieldcaption . "
                         </label>" . $controlhtml . $collapse .
                   "</div>";
               }
          return $output;
     }
     
     private function makeSaveDialog()
     {  return  "
          <div class='row'>
               <div class='help-block'>
               </div>
               <div class='centered'>
                    <button type='submit' class='btn btn-primary'>Bewaren
                    </button>
                    <a href='?soort=$this->soort' class='btn btn-default'>Annuleren
                    </a>
               </div>
          </div>";
     }
     
     private function makeOkDialog($tekst)
     {  return "
          <div class='row'>
               <div class='help-block'>
               </div>
               <div class='centered'>
                    <label for='okdialog_id'>" . $tekst . "
                    </label>                   
                    <div class='help-block'>
                    </div>
                    <a href='?soort=$this->soort' 
                       id='okdialog_id' 
                       class='btn btn-primary'>Ok
                    </a>
               </div>
          </div>";
     }
     
     private function getLookUpValues()
     {
          $result = array();
          foreach ($this->getHeader()->getColumns() as $column)
          {
               if (get_class($column) == "clsSelectColumn")
               {
                    $values = array();
                    foreach($this->connection->query($column->getLookUpSql()) as $row) 
                    { 
                         $values[$row['lookup_id']] = $row['lookupresult'];
                    }
                    $result[$column->getFieldName()] = $values;
               }
          } 
          return $result;
     }
     
     public function makeEditControls($keyrow = false)
     {
          $output = "";
          foreach ($this->getHeader()->getColumns() as $column)
          {    $output .= $this->makeFormControlSet($column, $keyrow);
          }
          return $output;
     }
     
     // Toegevoegd n.a.v. ticketnummer [[227]]
     private function noKeyValue()
     {
          $this->keyvalue = -1;		
          if (isset($_GET['key'])) 
          {    $this->keyvalue = $_GET['key'];
          }
          return ($this->keyvalue < 0);
     }
     // Einde toegevoegd n.a.v. ticketnummer [[227]]
     public function getEditHtml()
     {
          // Gewijzigd n.a.v. ticketnummer [[227]]
          if ($this->noKeyValue())
          {    return "Onbekende gegevens";
          }
          // Einde gewijzigd n.a.v. ticketnummer [[227]]
          
          $keyrow = false;
          foreach ($this->connection->query($this->getSelectSql()) as $key => $row) 
          {
               if ($row[$this->getKey()] == $key)
               {
                    $keyrow = $row;
                    break;
               }
          }
          if (!$keyrow)
          {    return $output;
          }
          
          $output = "
               <form action='?action=save&soort=$this->soort&key=" . $key . "' 
                     method='POST' 
                     role='form' 
                     class='form-horizontal'>" .
               $this->makeEditControls() .
               $this->makeSaveDialog();
          
          $output .= "
               </form>";
               
          return $output;
     }
     
     public function getNewHtml()
     {
          $output = "
               <form action='?action=insert&soort=$this->soort'  
                     method='POST' 
                     role='form' 
                     class='form-horizontal'>";
                     
          foreach ($this->getHeader()->getColumns() as $column)
          {    $output .= $this->makeFormControlSet($column);
          }
          
          $output .= $this->makeSaveDialog();
          
          $output .= "
               </form>";
               
          return $output;
     }
     
     public function getUpdateHtml()
     {
          // Gewijzigd n.a.v. ticketnummer [[227]]
          if ($this->noKeyValue())
          {    return $this->makeOkDialog("Onbekende gegevens."); 
          }
          // Einde gewijzigd n.a.v. ticketnummer [[227]]

          
          $error = $this->validationError($key);
          if ($error)
          {    return $this->makeOkDialog($error); 
          }
          
          $sql = "UPDATE " . $this->getTableName() . " SET ";
          // Submitted key-value pairs
          $fvpairs = array();
          foreach ($this->getHeader()->getColumns() as $column)
          {
               if (!$column->getReadOnly())
               {
                    $fvpairs[] = $column->getFieldName() . " = '" . 
                                 $column->setFieldValueToDB($_POST[$column->getFieldName() . "_id"]) . "'";
               }
          }
          
          $sql .= join(', ', $fvpairs) . // Convert key-value pairs to comma separated string
                 "   WHERE " . $this->getKey() . " = '" . $this->getKeyValue() . "'";
          
          // Gewijzigd n.a.v. ticketnummer [[219]]
          if ($this->connection->query($sql) == true)
          {    return $this->makeOkDialog("De gegevens zijn opgeslagen.");
		} 
		else 
		{    return $this->makeOkDialog($sql . " is mislukt. De gegevens zijn NIET opgeslagen"); 
		     die;
		}							
          // Einde gewijzigd n.a.v. ticketnummer [[219]]
     }
     
     public function getInsertHtml()
     {
          $error = $this->validationError(-1);
          if ($error)
          {    return $this->makeOkDialog($error); 
          }
          
          // Submitted fields and values
          $fields = array();
          $values = array();
          foreach ($this->getHeader()->getColumns() as $column)
          {
               if (!$column->getReadOnly())
               {
                    $fields[] = $column->getFieldName();
                    $values[] = $column->setFieldValueToDB($_POST[$column->getFieldName() . "_id"]);
               }
          }
          $sql = 
               "INSERT INTO " . $this->getTableName() . 
                          " (" . join(', ', $fields) . ") 
                     VALUES ('" . join("', '", $values) . "')";  
                     
          // Gewijzigd n.a.v. ticketnummer [[220]]
          if ($this->connection->query($sql) == true)
          {    return $this->makeOkDialog("De gegevens zijn opgeslagen.");
		} 
		else 
		{    return $this->makeOkDialog($sql . " is mislukt. De gegevens zijn NIET opgeslagen"); 
		     die;
		}							
          // Einde gewijzigd n.a.v. ticketnummer [[220]]
     }
     
     public function getDeleteHtml()
     {
          // Gewijzigd n.a.v. ticketnummer [[227]]
          if ($this->noKeyValue())
          {    return $this->makeOkDialog("Onbekende gegevens."); 
          }
          // Einde gewijzigd n.a.v. ticketnummer [[227]]

          $sql =  "DELETE FROM " . $this->getTableName();
          $sql .= " WHERE " . $this->getKey() . " = '" . $this->getKeyValue() . "'";
          
          // Gewijzigd n.a.v. ticketnummer [[217]]          
          if ($this->connection->query($sql) == true)
          {    return $this->makeOkDialog("De gegevens zijn verwijderd.");
		} 
		else 
		{    return $this->makeOkDialog($sql . " is mislukt. De gegevens zijn NIET verwijderd"); 
		     die;
		}							
          // Einde gewijzigd n.a.v. ticketnummer [[217]]          
     }

     public function getTableHtml()
     {
          $output = 
              "<table>
                    <thead>";
          if ($this->title)
          {
               $output .=
                        "<tr>
                              <th colspan='1000'>" . $this->title . "</th>
                         </tr>";
          }
          $output .=    "<tr>";
          foreach ($this->getHeader()->getColumns() as $column)
          {
               $output .=    "<th>" . $column->getCaption() . "</th>";
          }
          if (!$this->readonly)
          {    
               $output .=    "<th colspan='2' class='text-center'>
                                   <a href='?action=new&soort=$this->soort'>
                                        <i class='fa fa-plus'></i>
                                   </a>
                              </th>";
          }
          $output .= "   </tr>
                    </thead>
                    <tbody>";
                    
          
          $lookupvalues = $this->getLookupValues();
          
          foreach ($this->connection->query($this->getSelectSql()) as $row) 
          {
               $output .= 
                        "<tr " . $this->makeRowClass($row) . ">";
               foreach ($this->getHeader()->getColumns() as $column)
               {
                    $output .= 
                             "<td>";
                    $fldname = $column->getFieldName();
                    if (!$column->getColumnHtml())
                    {    if (get_class($column) == "clsSelectColumn")
                         {    // Lookup field
                              $fldvalue = 
                                       $lookupvalues[$fldname][$row[$fldname]];
                         }
                         else
                         {    $fldvalue = $column->getFieldValueFromDB($row);
                         }
                         $output .= $fldvalue;
                    }
                    else
                    { // HTML 
                         $output .= $column->parseFieldValues($row);
                    }
                    $output .= 
                             "</td>";
               }
               if (!$this->readonly)
               {    $output .=    
                             "<td>
                                   <a href='?action=edit&soort=$this->soort&key=" . $row[$this->getKey()]."'>
                                        <i class='fa fa-pencil'></i>
                                   </a>
                              </td>
                              <td>
                                   <a href='?action=delete&soort=$this->soort&key=" . $row[$this->getKey()]."'>
                                        <i class='fa fa-trash-o'></i>
                                   </a>
                              </td>";
               }
               $output .= 
                        "</tr>";
          }
          $output .= "
                    </body>
               </table>";         
          return $output;
     }
     
     protected function makeRowClass($row)
     {
          return ""; // Kan worden overschreven in de afgeleide klasse
     }
     
     protected function validationError($key)
     {
          return false; // Kan worden overschreven in de afgeleide klasse
     }
}
?>