<?php
require_once("includes/includes.php");
require_once(CLASSES_PATH . "table.inc.php");	

class clsBestelling 
{
     public function __construct() 
     {
          //Call the databaseconnection
          $this->connection = database::connect();
     }
     
     public function getBestelling() 
     {
          if (isset($_GET['reservering'])) 
          {
               $_SESSION['bestellingen']['reserverings_id'] = $_GET['reservering'];
          }
          if (isset($_GET['action'])) 
          {
               $action = $_GET['action'];
               switch($action) 
               {
                    case "plusitem"	: $this->plusitem(); break;
                    case "minitem"		: $this->minitem(); break;
                    case "deleteitem"	: $this->deleteitem(); break;
               }
               
          }
          
          if (isset($_SESSION['bestellingen']['reserverings_id'])) 
          {
               if ($_SESSION['bestellingen']['reserverings_id'] > 0) 
               {
                    return $this->currentBestelling();
               }
          }
     }
     
     protected function getTafelnummer() 
     {
          if (isset($_GET['tafelnummer'])) 
          {
               if ($_GET['tafelnummer'] > 0) 
               {
                    $_SESSION['bestellingen']['tafelnummer'] = $_GET['tafelnummer'];
               }
          }
          $output = "Bestelling voor tafel " . $_SESSION['bestellingen']['tafelnummer'];
          return $output;
     }
     
     private function getCurrentAantal() 
     {
          $reservering_id 	= $_GET['reservering'];
          $menuitemcode 		= $_GET['menuitemcode'];
          $sql 			= "SELECT aantal FROM bestelling 
                                 WHERE reservering_id = $reservering_id 
                                   AND menuitemcode = '$menuitemcode'"; // Gewijzigd n.a.v. ticketnummer [[226]]
                              
          $stmt			= $this->connection->prepare($sql); 
          $stmt->execute(); 
          $row 		     = $stmt->fetch();												
          $aantal 			= $row['aantal'];
          return $aantal;
     }
     
     
     private function plusitem() 
     {
          $reservering_id = $_GET['reservering'];
          $menuitemcode = $_GET['menuitemcode'];
                         
          $aantal = $this->getCurrentAantal();
          $aantal++;
          
          $sql = "UPDATE bestelling SET aantal = $aantal 
                  WHERE reservering_id = $reservering_id 
                    AND menuitemcode = '$menuitemcode'";
          // Gewijzigd n.a.v. ticketnummer [[214]]
          if ($this->connection->query($sql) == true) 
          {
               return;
          } 
          else 
          {
               print $sql . " Mislukt"; die;
          }							
          // Einde gewijzigd n.a.v. ticketnummer [[214]]
     }
     
     private function minitem() 
     {
          $reservering_id = $_GET['reservering'];
          $menuitemcode = $_GET['menuitemcode'];
          
          // Gewijzigd n.a.v. ticketnummer [[225]]
          $aantal = $this->getCurrentAantal();
          if ($aantal > 0) 
          {
               $aantal--;
          } 
          else 
          {
               return;
          }
          // Einde gewijzigd n.a.v. ticketnummer [[225]]
          
          $sql = "UPDATE bestelling SET aantal = $aantal 
                  WHERE reservering_id = $reservering_id 
                    AND menuitemcode = '$menuitemcode'";
                    
          // Gewijzigd n.a.v. ticketnummer [[215]]
          if ($this->connection->query($sql) == true) 
          {
               return;
          } 
          else 
          {
               print $sql . " Mislukt"; die;
          }			
          // Einde gewijzigd n.a.v. ticketnummer [[215]]
     }
     
     private function deleteitem() 
     {
          $reservering_id = $_GET['reservering'];
          $menuitemcode = $_GET['menuitemcode'];
          
          $sql = "DELETE FROM bestelling 
                   WHERE reservering_id = $reservering_id 
                     AND menuitemcode = '$menuitemcode'";
          
          // Gewijzigd n.a.v. ticketnummer [[216]]
          if ($this->connection->query($sql) == true) 
          {
               return;
          } 
          else 
          {
               print $sql . " Mislukt"; die;
          }
          // Einde gewijzigd n.a.v. ticketnummer [[216]]
     }
     
     
     private function currentBestelling() 
     {
          $reserveringsID = $_SESSION['bestellingen']['reserverings_id'];
          
          $sql = "SELECT * FROM bestelling b, menuitem m 
                   WHERE b.reservering_id = $reserveringsID
                     AND b.menuitemcode = m.menuitemcode";

          $output = "
               <table>
                    <thead>
                         <tr>
                              <th>" . $this->getTafelnummer() . "</th>
                              <th colspan='4' class='text-right'>
                                   <a href='reserveringen.php?reservering=$reserveringsID'class='btn btn-default
                                   fa fa-arrow-left'>&nbsp;&nbsp;Terug</a>
                              </th>
                         </tr>
                    </thead>
                    <tbody>";
               
                    foreach ($this->connection->query($sql) as $row) 
                    {
                         $menuitemcode = $row['menuitemcode'];
                         $output .= "
                         <tr>
                              <td>" . $row['menuitemnaam'] . "</td>
                              <td>" . $row['aantal'] . "</td>
                              <td>
                                   <a href='bestellingen.php?action=plusitem&menuitemcode=$menuitemcode
                                                            &reservering=$reserveringsID'>
                                        <i class='fa fa-plus-circle' aria-hidden='true'></i>
                                   </a>
                              </td>
                              <td>
                                   <a href='bestellingen.php?action=minitem&menuitemcode=$menuitemcode
                                                            &reservering=$reserveringsID''>
                                        <i class='fa fa-minus-circle' aria-hidden='true'></i>
                                   </a>
                              </td>
                              <td>
                                   <a href='bestellingen.php?action=deleteitem&menuitemcode=$menuitemcode
                                                            &reservering=$reserveringsID''>
                                        <i class='fa fa-trash-o' aria-hidden='true'></i>
                                   </a>
                              </td>
                         </tr>";
                    }
          
          $output .= "
                    </tbody>
               </table>
               <br />
               <a href='bon.php?reservering=$reserveringsID' 
                  class='btn btn-default' 
                  target='_blank'>Print bon voor klant
               </a>";
          return $output;        
     }
}	


class clsPage extends clsDefaultPage
{
     
     public function contentHtml() 
     {
          if (isset($_GET['action'])) 
          {
               if ($_GET['action'] == "nieuw") 
               {
                    $_SESSION['bestellingen']['tafelnummer'] = '-1';
                    header("Location: bestellingen.php");
               }
               
               if ($_GET['action'] == "add") 
               {
                    $this->save();
                    header("Location: bestellingen.php");				
               }
          }
          $bestelling = new clsBestelling;
          $output = '
               <div class="col-xs-6 col-sm-6 col-lg-6">' .
               $bestelling->getBestelling() .
              '</div>
               <div class="col-xs-6 col-sm-6 col-lg-6">' .
                    $this->getGerecht() .
              '</div>';
          return $output;
     }
     
     protected function getMenuitem($menuitemcode) 
     {
          $sql = "SELECT * FROM menuitem WHERE menuitemcode = '$menuitemcode'";
          foreach ($this->connection->query($sql) as $row) 
          {
               $aOutput = $row;
          }
          return $aOutput;
     }
     
     
     protected function save() 
     {
          // insert one row
          $tafel 			= $_SESSION['bestellingen']['tafelnummer'];
          $reservering_id	= $_SESSION['bestellingen']['reserverings_id'];
          $datum 			= date("Y-m-d");
          $tijd 			= date("H:i:s");
          $menuitemcode 		= $_GET['item'];
          $aMenuitem 		= $this->getMenuitem($menuitemcode);
          $aantal 			= 1;
          $prijs 			= $aMenuitem['prijs'];
          
          $sql = "INSERT INTO bestelling 
                              (reservering_id, tafel, datum, tijd, menuitemcode, aantal, prijs) 
                       VALUES ($reservering_id, $tafel, '$datum', '$tijd', '$menuitemcode', $aantal, $prijs)";
                         
          // Gewijzigd n.a.v. ticketnummer [[218]]          
          if (!($this->connection->query($sql) == true)) 
		{    print $sql . " Mislukt"; die;
		}							
          // Einde gewijzigd n.a.v. ticketnummer [[218]]          
     }
     
     protected function getGerecht() 
     {
          $output = "";
          //GERECHT
          
          foreach($this->connection->query('SELECT * FROM gerecht') as $rowgerecht) 
          {
              $output .= 
                   "<div class='row'><h3>" . $rowgerecht['gerechtnaam'] . "</h3>"; // Gewijzigd n.a.v. ticketnummer [[224]]
                         
              //SUBGERECHT
              foreach ($this->connection->query('SELECT * FROM subgerecht') as $rowsubgerecht) 
              {
                   if ($rowgerecht['gerechtcode'] == $rowsubgerecht['gerechtcode']) 
                   {
                        $output .= "
                        <div class='col-xs-2 col-md-2'><h5>" . $rowsubgerecht['subgerechtnaam'] . "</h5>";
                         
                        //MENUITEM
                        foreach ($this->connection->query('SELECT * FROM menuitem') as $rowmenuitem) 
                        {
                             if ($rowsubgerecht['subgerechtcode'] == $rowmenuitem['subgerechtcode']) 
                             {
                                  $output .= "
                              <a href='?action=add&item=" . $rowmenuitem['menuitemcode'] ."'>"
                               . $rowmenuitem['menuitemnaam'] 
                           . "</a><br />";
                             }
                        }   	
                        $output .= "
                              </div>"; //end div subgerecht
                   }
              }
              $output .= "
                    </div>"; //end div gerecht
          }			
          return $output;
     }		
}
     $page = new clsPage();
	echo $page->getHtml();

?>