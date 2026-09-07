<?php
	
class clsMainMenu 
{
     public function getHtml() 
     {
          $output = '
          <nav class="navbar navbar-default">
            <div class="container-fluid">

              <!-- Collect the nav links, forms, and other content for toggling -->
              <div class="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                <ul class="nav navbar-nav">
                  
                  <!--HOME-->
                  <li class=""><a href="index.php">Home</a></li>
                  
                  <!--RESERVERINGEN-->
                  <li class=""><a href="reserveringen.php">Reserveringen</a></li>
                  
                   <!--BESTELLINGEN
                  <li class="">
                    <a 	href="bestellingen.php" 
                              class="dropdown-toggle" 
                              data-toggle="dropdown" 
                              role="button" 
                              aria-haspopup="true" 
                              aria-expanded="false">Bestellingen <span class="caret"></span>
                    </a>
                  </li>-->
                  
                  <!--OVERZICHTEN-->
                  <li class="dropdown">
                    <a 	href="overzichten.php" 
                              class="dropdown-toggle" 
                              data-toggle="dropdown" 
                              role="button" 
                              aria-haspopup="true" 
                              aria-expanded="false">Overzichten <span class="caret"></span>
                    </a>
                         <ul class="dropdown-menu">
                           <li><a href="overzichten.php?overzicht=kok">Voor kok</a></li>
                           <li><a href="overzichten.php?overzicht=ober">Voor ober</a></li>
                         </ul>
                  </li>
                  
                  <!--GEGEVENS-->
                  <li class="dropdown">
                    <a 	href="gegevens.php" 
                              class="dropdown-toggle" 
                              data-toggle="dropdown" 
                              role="button" 
                              aria-haspopup="true" 
                              aria-expanded="false">Gegevens <span class="caret"></span>
                    </a>
                         <ul class="dropdown-menu">
                           <li><a href="gegevens.php?soort=drinken">Drinken</a></li>
                           <li><a href="gegevens.php?soort=eten">Eten</a></li>
                           <li><a href="gegevens.php?soort=klanten">Klanten</a></li>
                           <li><a href="gegevens.php?soort=gerechten">Gerecht hoofdgroepen</a></li>
                           <li><a href="gegevens.php?soort=subgerechten">Gerecht subgroepen</a></li>
                         </ul>
                  </li>
                </ul><!-- end class="nav navbar-nav"-->
                
              </div><!-- /.navbar-collapse -->
            </div><!-- /.container-fluid -->
          </nav>
          ';
          return $output;
     }
}