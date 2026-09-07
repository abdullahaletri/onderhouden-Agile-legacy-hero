<?php
	
	/**
	 * footer class
	 * Sets the footer at end of page for every requested page
	 */
	class footer {	
		public function getHtml() {
			$output = '<i>Stichting Praktijkleren</i> Amersfoort &copy 2017';
			return $output;
		}
	}