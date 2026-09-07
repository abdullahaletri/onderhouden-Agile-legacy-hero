<?php
	require_once("includes/includes.php");

     class clsPage extends clsDefaultPage
     {
          protected function contentHtml()
          {
               return '
                    <div class="col-xs-5 col-sm-5 col-lg-5">
                    <ul>
                         <li class="nobullet">
                              <p>
                                   Welkom bij de reserverings- en bestellingenapplicatie van Restaurant Excellent Taste.
                              </p>
                              <p> 
                                   Vul eerst een reservering in. Deze kan telefonisch binnenkomen of kan worden 
                                   ingevoerd als gasten plaatsnemen aan een vrije tafel.
                              </p>
                              <p>
                                   Daarna kan een bestelling worden opgenomen.
                              </p>
                         </li>
                    </ul>
                    </div>
                    <div class="col-xs-7 col-sm-7 col-lg-7">
                         <img src="images/restaurant.jpg" class="img-responsive">
                    </div>';
          }
     }
     
     $page = new clsPage();
	echo $page->getHtml();
?>