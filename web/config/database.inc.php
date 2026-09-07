<?php
	class database 
	{
		public static function connect() 
		{
			$connection = new PDO('mysql:host=localhost;dbname=excellenttaste_db;charset=utf8', 'root', '');
			return $connection;
		}
	}
?>