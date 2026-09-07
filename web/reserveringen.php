<?php

require_once("includes/includes.php");
require_once(CLASSES_PATH . "table.inc.php");
require_once(CLASSES_PATH . "klanten.inc.php");
	
class clsReserveringen extends clsTableDef
{     
     public function __construct() 
     {
          parent::__construct();
                   
          $vandaag = date("Y-m-d");
          $this->selectsql = "SELECT reservering_id, tafel, datum, 
                                     tijd, aantal, klantnaam, r.klant_id 
                                FROM reservering r 
                                LEFT JOIN klant k on r.klant_id = k.klant_id 
                              ORDER BY datum, tijd, tafel"; 
          $this->tablename = "reservering";
          $this->key = "reservering_id";
          $this->title = "<p class='text-center'>| rood = verleden | groen = vandaag | oranje = toekomst |</p>";
          
          $column = new clsDateColumn();
          $column->setFieldName("datum");
          $column->setCaption("Datum");         
          $column->setEditType("Date");
          $this->columnHeader->addColumn($column);
          
          $column = new clsColumn();
          $column->setFieldName("tijd");
          $column->setCaption("Tijd"); 
          $this->columnHeader->addColumn($column);
          
          $column = new clsColumn();
          $column->setFieldName("tafel");
          $column->setCaption("Tafel");         
          $this->columnHeader->addColumn($column);
          
          $column = new clsKlantSelect;
          $column->setFieldName("klant_id");
          $column->setCaption("Klant");
          $column->setEditType("Select");
          $column->setLookUpSql("SELECT klantnaam AS lookupresult, klant_id AS lookup_id  
                                   FROM klant");
          $this->columnHeader->addColumn($column);
          
          $column = new clsColumn();
          $column->setFieldName("aantal");
          $column->setCaption("Aantal");         
          $this->columnHeader->addColumn($column);
          
          $column = new clsColumn();
          $column->setFieldName("");
          $column->setColumnHtml("<a href='bestellingen.php?reservering=<!reservering_id!>&tafelnummer=<!tafel!>'class='btn btn-default'>Bestelling</a>");
          $this->columnHeader->addColumn($column);          
     } 
     
     protected function makeRowClass($row)
     {
          $vandaag = date("Y-m-d");
          $class = "vandaag";
          if ($row['datum'] > $vandaag) 
          {    $class = "toekomst"; 
          }
          else
          {    if ($row['datum'] < $vandaag) 
               {    $class = "verleden"; 
               }
          }
          return "class='$class'";
     }
     
     protected function validationError($key)
     {    $output = false;
          $tafel  = $_POST['tafel_id'];
          $datum  = $_POST['datum_id'];         
          $tijd   = $_POST['tijd_id'];

          $aantal_tdt = 1; 
          $sql = "SELECT COUNT(*) AS aantal_tdt FROM reservering  
                   WHERE (tafel = $tafel)
                     AND (DATE_FORMAT(datum, '%d-%m-%Y') = '$datum')
                     AND (tijd = '$tijd')";
          foreach ($this->connection->query($sql) as $row) 
          {
               $aantal_tdt = $row['aantal_tdt'];
          }
          if ($aantal_tdt > 0)
          {
               $output = "Deze tafel is op dit tijdstip al bezet!";
          }
          return $output;
     }
     
}

class clsPage extends clsDefaultPage
{
     protected function contentHtml() 
     {			
          $keyvalue = false;		
          if (isset($_GET['key'])) 
          {    $keyvalue = $_GET['key'];
          }  
          $this->datalist = new clsReserveringen();
          $this->datalist->setSoort("reserveringen");
          $this->datalist->setKeyValue($keyvalue);
          return $this->getActionHtml();
     }
     
     private function add() 
     {    $klantId = -1;
          if (isset($_POST['inputklanten'])) 
          {
               $klantId = $_POST['inputklanten'];
          }
          $row = $this->getKlantVars($klantId); 
          $rowreservering = $this->getReserveringVars(-1);
          $row = array_merge($row, $rowreservering);

          $output = "
               <form action='?action=add' method='post' enctype='multipart/form-data'>

                    <label>Selecteer klant
                    </label>
                    
                    <select id='inputklanten' name='inputklanten'>" .
                         $this->getKlanten() . "                         
                    </select> 
                     
                    <input type='submit' name='submit_klantselecteren' 
                           id='submit_klantselecteren' value='Selecteer klant'/>
               </form>" . $this->showReserveringForm($row);
          
          return $output;	
     }
     
     protected function edit()
     {
          $reserveringId = -1;
          if (isset($_GET['reservering'])) 
          {
               $reserveringId = $_GET['reservering'];
          }
          
          $row = $this->getReserveringVars($reserveringId); 
          $klantId = $row['klant_id'];
          $rowklant = $this->getKlantVars($klantId); 
          $row = array_merge($row, $rowklant);
          
          $output = $this->showReserveringForm($row);
         
          return $output;
     }
     
     private function save() 
     {
          $reservering_id = -1;
          if (isset($_POST['reserveringId'])) 
          {
               $reservering_id = $_POST['reserveringId'];
          }
          //check of de klant nieuw is of al bestaat	
          $klant_id	= $this->saveNewCustomer($_POST['klantId']);
          
          //variabelen vullen met de juiste waarden
          $tafel  = $_POST['tafelnummer'];
          $datum  = $_POST['reserveringsdatum'];         
          //de datum moet omgedraaid worden voordat die opgeslagen wordt
          $datum  = $this->convertDateSQLFormat($datum, -1);
          
          $tijd   = $_POST['reserveringstijd'];
          $aantal = $_POST['aantal'];
          
          $aantal_tdt = 1; 
          $sql = "SELECT COUNT(*) AS aantal_tdt FROM reservering  
                   WHERE tafel = $tafel AND datum = '$datum' AND tijd = '$tijd'";
          foreach($this->connection->query($sql) as $row) 
          {
               $aantal_tdt = $row['aantal_tdt'];
          }
          
          if ($reservering_id < 0)
          {
               if ($aantal_tdt > 0)
               {
                    return $this->makeFoutDialog("Deze tafel is al bezet op de gevraagde tijd.");
               }
               $sql = "INSERT INTO reservering 
                                   (tafel, datum, tijd, klant_id, aantal) 
                                   VALUES 
                                   ($tafel, '$datum', '$tijd', $klant_id, $aantal)";
          }
          else
          {
               $sql = "UPDATE reservering  
                          SET tafel = $tafel, datum = '$datum', tijd = '$tijd', aantal = $aantal  
                        WHERE reservering_id = $reservering_id";
          }
          // Gewijzigd n.a.v. ticketnummer [[212]]
          if ($this->connection->query($sql) == true) 
          {    header("Location: reserveringen.php");
		} 
		else 
		{    print $sql . " Mislukt"; die;
		}
		// Einde gewijzigd n.a.v. ticketnummer [[212]]
     }   

     protected function delete() 
     {
          $reservering_id = -1;
          if (isset($_POST['reservering'])) 
          {
               $reservering_id = $_POST['reservering'];
          }
          $sql = "DELETE FROM reservering  
                   WHERE reservering_id = $reservering_id";
          // Gewijzigd n.a.v. ticketnummer [[213]]
          if ($this->connection->query($sql) == true) 
          {    header("Location: reserveringen.php");
		} 
		else 
		{    print $sql . " Mislukt"; die;
		}	
		// Einde gewijzigd n.a.v. ticketnummer [[213]]
     }
     
     protected function bestellen() 
     {
          if(isset($_GET['reservering'])) 
          {
               if($_GET['reservering'] > 0) 
               {
                    $reserveringsnummer = $_GET['reservering'];
                    $_SESSION['bestellingen']['reserveringsnummer'] = $reserveringsnummer;
                    
                    $sql = "SELECT tafel FROM reservering 
                                   WHERE reservering_id = '$reserveringsnummer'";

                    foreach($this->connection->query($sql) as $row) 
                    {
                         $_SESSION['bestellingen']['tafelnummer'] = $row['tafel'];
                    }
                    header("Location: bestellingen.php?reservering=$reserveringsnummer");
               }
          }
     }
     
     private function getKlanten() 
     {
          $sql = "SELECT klant_id, klantnaam FROM klant
                  WHERE status = '1'";
          $output = "<option id='-1' value='-1'>--Nieuwe klant--</option>";
          foreach($this->connection->query($sql) as $row) 
          {
               $output .= "<option id='" . $row['klant_id'] . "' value='" . 
                          $row['klant_id'] . "'> " . $row['klantnaam'] ."</option>";
          }
          return $output;
     }
     
     private function getKlantVars($klantId) 
     {
          if ($klantId > -1)
          {
               $sql = "SELECT klant_id, klantnaam, email, telefoon FROM klant
                       WHERE klant_id = $klantId 
                         AND status = '1'"; // Gewijzigd n.a.v. ticketnummer [[221]]
               $stmt = $this->connection->prepare($sql); 
               $stmt->execute(); 
               $row	= $stmt->fetch();												
          }
          else
          {    // Nieuwe klant
               $row['klantnaam']	= "";
               $row['email']		= "";
               $row['klant_id']	= -1;
               $row['telefoon']	= "";
          }
          return $row;			
     }
     private function getReserveringVars($reserveringId) 
     {
          if ($reserveringId > -1)
          {
               $sql = "SELECT reservering_id, tafel, datum, tijd, klant_id, aantal FROM reservering
                       WHERE reservering_id = $reserveringId 
                         AND status = '1'"; // Gewijzigd n.a.v. ticketnummer [[222]]
               $stmt = $this->connection->prepare($sql); 
               $stmt->execute(); 
               $row	= $stmt->fetch();
               $row['datum'] = $this->convertDateSQLFormat($row['datum'], 1);
          }
          else
          {    // Nieuwe reservering
               $row['reservering_id'] = -1;
               $row['datum']	        = "";
               $row['tijd']	        = "";
               $row['tafel']	        = "";
               $row['aantal']	        = "";
          }
          return $row;			
     }
     
     
     private function showReserveringForm($row)
     {    $dis_html = "disabled";
          if ($row['klant_id'] < 0)
          {
               $dis_html = "";
          }
          $output = "
               <form action='?action=save' method='post' enctype='multipart/form-data'>
                    <input type='hidden' name='klantId' id='klantId' value='" . $row['klant_id'] . "' />
                    <input type='hidden' name='reserveringId' id='reserveringId' value='" . $row['reservering_id'] . "' />
                    
                    <label>Klantnaam</label>
                    <input type='text' name='klantnaam' id='klantnaam' $dis_html value='" . $row['klantnaam'] . "' />
                    
                    <label>E-mailadres klant</label>
                    <input type='email' name='email' id='email' $dis_html value='" . $row['email'] . "'/>
                    
                    <label>Telefoonnummer klant</label>
                    <input type='text' name='telefoon' id='telefoon' $dis_html value='" . $row['telefoon'] . "'/>
                    
                    <label>Reserveringsdatum (dd-mm-jjjj)</label>
                    <input type='text' name='reserveringsdatum' id='reserveringsdatum' value='" . $row['datum'] . "' />
                    
                    <label>Reserveringstijd (uu:mm)</label>
                    <input type='text' name='reserveringstijd' id='reserveringstijd' value='" . $row['tijd'] . "' />
                    
                    <label>Tafelnummer</label>
                    <input type='number' name='tafelnummer' id='tafelnummer' min='1' max='10' value='" . $row['tafel'] . "' />
                    
                    <label>Aantal personen</label> 
                    <input type='text' name='aantal' id='aantal' value='" . $row['aantal'] . "' />
                    
                    <label></label>
                    <input type='submit' name='submit' id='submit' value='Opslaan' />
               </form>";
          return $output;
     }
     
     private function saveNewCustomer($klant_id) 
     {
          $id = $klant_id;
          if ($id < 0)
          {    //save customer first and return klant_id
               $klantnaam 	= $_POST['klantnaam'];
               $email 		= $_POST['email'];
               $telefoon	     = $_POST['telefoon']; // Gewijzigd n.a.v. ticketnummer [[223]]
               
               $sql = "INSERT INTO klant 
                                   (klantnaam, email, telefoon) 
                              VALUES 
                                   (:klantnaam, :email, :telefoon)";
                    
               $stmt = $this->connection->prepare($sql);
               $stmt->bindParam(':klantnaam', $klantnaam);
               $stmt->bindParam(':email', $email);
               $stmt->bindParam(':telefoon', $telefoon);
               $stmt->execute();
               $id = $this->connection->lastInsertId();
          }
          return $id; //if not new saved klant, then return existed klant_id
     }
     
     private function convertDateSQLFormat($datum, $toSQL) 
     {
          if ($toSQL < 0)
          {
               // 15-02-2017 to 2017-02-15
               $dag 	= substr($datum, 0, 2);
               $maand 	= substr($datum, 3, 2);
               $jaar	= substr($datum, 6, 4);
               $output = $jaar . "-" . $maand . "-" . $dag;
          }
          else
          {
               // 2017-02-15 to 15-02-2017
               $jaar 	= substr($datum, 0, 4);
               $maand 	= substr($datum, 5, 2);
               $dag	= substr($datum, 8, 2);
               $output = $dag . "-" . $maand . "-" . $jaar;
          }
          return $output;
     }
         
     protected function getReserveringen() 
     {
          $output = '<p>Klik op het tafelnummer om een bestelling te maken.</p>';
          $output .= '| rood = verleden | groen = vandaag | oranje = toekomst |'; // Toegevoegd n.a.v. ticketnummer [[255]]
          $output .= '<table>';
               $output .= '<thead>';
                    $output .= '<tr>';
                         $output .= '<th>Datum</th>';
                         $output .= '<th>Tijd</th>';
                         $output .= '<th>Tafel</th>';
                         $output .= '<th>Klantnaam</th>';
                         $output .= '<th>Telefoonnummer</th>';
                         $output .= '<th>Aantal</th>';
                         $output .= '<th>
                                          <a href="?action=add">
                                               <i class="fa fa-plus" aria-hidden="true"></i>
                                          </a>
                                     </th>';
                         $output .= '<th></th>';
                    $output .= '</tr>';
               $output .= '</thead>';
               
               $output .= '<tbody>';

                    $sql = "SELECT * FROM reservering r, klant k 
                            WHERE r.klant_id = k.klant_id
                              AND r.status = '1'
                            ORDER BY datum, tijd";

                    foreach($this->connection->query($sql) as $row) {
                         
                         $class = "";
                         if($row['datum'] > date("Y-m-d")) { $class = "toekomst"; }
                         if($row['datum'] == date("Y-m-d")) { $class = "vandaag"; }
                         if($row['datum'] < date("Y-m-d")) { $class = "verleden"; }
                         
                         $output .= "<tr class='$class'>";
                              $output .= "<td>";
                                   //Rotate date from sql to dutch notation
                                   $datum 	= $row['datum'];
                                   $dag 	= substr($datum, 8, 2);
                                   $maand 	= substr($datum, 5, 2);
                                   $jaar 	= substr($datum, 0, 4);
                                   $datum 	= $dag . "-" . $maand . "-" . $jaar;
                                   
                                   $output .= $datum;
                              $output .= "</td>";
                              
                              $output .= "<td>";
                                   $output .= substr($row['tijd'],0,5); //We only need hours and seconds
                              $output .= "</td>";
                              
                              $output .= "<td class='td_tafel'>";
                                   $output .= "<a href='?action=bestellen&reservering=" . $row['reservering_id'] . " '>" . $row['tafel'] . "</a>";
                              $output .= "</td>";

                              
                              $output .= "<td>";
                                   $output .= $row['klantnaam'];
                              $output .= "</td>";
                              
                              $output .= "<td>";
                                   $output .= $row['telefoon'];
                              $output .= "</td>";
                              
                              $output .= "<td>";
                                   $output .= $row['aantal'];
                              $output .= "</td>";
                              
                              $output .= "<td>";
                                   $output .= "<a href='?action=edit&reservering=" . $row['reservering_id']."'><i class='fa fa-pencil'></i></a>";
                              $output .= "</td>";

                              $output .= "<td>";
                                   $output .= "<a href='?action=delete&reservering=" . $row['reservering_id']."'><i class='fa fa-trash-o'></a>";
                              $output .= "</td>";

                         $output .= "</tr>";
                    }
          
          $output .= "</tbody>";
          $output .= "</table>";
          return $output;
     }	
}
     $page = new clsPage();
	echo $page->getHtml();

?>