<?php 
	$dbUsername = "davi4403_wpview";
	$dbPassword = "5ta4utkj1xx6";
	$db = "davi4403_wp7";
	$server = "localhost";
	
	$lastUpdate = $_GET['lastUpdate'];
	
	//FWP Table's Column Names
	$mysql_jokeTable = "Jokes";
	$mysql_categoryTable = "Categories";
    $mysql_id = "id";
	$mysql_joke = "joke";
	$mysql_author = "author";
	$mysql_statistic = "statistic";
	$mysql_statisticURL = "statisticURL";
	$mysql_dateAdded = "dateAdded";
	$mysql_charity = "charity";
	$mysql_charityURL = "charityURL";
	$mysql_favorite = "favorite";
	$mysql_categoryID = "categoryID";
	$mysql_categoryText = "categoryText";
	$mysql_viewCategoryFilter ="viewCategoryFilter";
    
    $connection = mysql_connect($server, $dbUsername, $dbPassword);
	
	if($lastUpdate!="")
	{
		$itemQueryAddition = "WHERE dateAdded > '$lastUpdate'";
	}
	
	function formatInput($rawURLData)
	{
		$returnString = urldecode($rawURLData); 
		$returnString = mysql_real_escape_string($returnString);
		return $returnString;
	}
?>
