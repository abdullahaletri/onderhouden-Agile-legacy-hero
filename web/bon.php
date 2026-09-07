<?php

require_once("includes/includes.php");
require_once(CLASSES_PATH . "table.inc.php");	

class clsPage extends clsDefaultPage
{
     protected function contentHtml() 
     {			
          $aBestelling = $this->getBestelling();
          $totaal = 0;
          $output = 
               "<center>
                     <button class='btn btn-primary hidden-print aria-hidden='true'' 
                             onclick='window.print()'>
                          <span class='glyphicon glyphicon-print'></span> Print bon
                     </button>
	           </center>
                <div id='bon_overzicht'>
                     <table id='bon'>
                          <thead>
                               <tr>
                                    <th>Product</th>
                                    <th>Aantal</th>
                                    <th>Prijs p/s</th>
                                    <th>Totaal</th>
                               </tr>
                          </thead>
                          <tbody>";
          
          foreach ($aBestelling as $key => $value) 
          {
                $output .= "   <tr>
                                    <td>" . $value['menuitemnaam'] . "</td>
                                    <td>" . $value['aantal'] . "</td>
                                    <td>&euro;&nbsp;" . number_format($value['prijs'], 2, ',','.') . "</td>
                                    <td>&euro;&nbsp;" . number_format($value['aantal'] * 
                                                                      $value['prijs'], 2, ',','.') . "</td>
                               </tr>";
                $totaal = $totaal + ($value['aantal'] * $value['prijs']);
          }
          $output .=          "<tr class='trBold'>
                                    <td>TOTAALPRIJS</td>
                                    <td colspan='3'>&euro;&nbsp;" . number_format($totaal, 2, ',','.') . "</td>
                               </tr>
                          </tbody>
                     </table>
                </div>";
          return $output;
     }	
     
     private function getBestelling() 
     {
          $reservering_id = $_GET['reservering'];
          $sql = "SELECT * FROM bestelling b, menuitem m 
                   WHERE b.reservering_id = $reservering_id
                     AND b.menuitemcode = m.menuitemcode"; // Gewijzigd n.a.v. ticketnummer [[241]]
          foreach($this->connection->query($sql) as $row) 
          {
               $aOutput[] = $row;
          }
          return $aOutput;
     }
}
     $page = new clsPage();
	echo $page->getHtml();
?>