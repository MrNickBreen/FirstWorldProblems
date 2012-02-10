<?
	include 'mysql_vars.php';
	
	// Construct our MySQL query
	
	$jokeQuery = "SELECT * FROM `$mysql_jokeTable` $itemQueryAddition ORDER BY `$mysql_dateAdded` DESC;";

	// execute the query and gather the results...
	mysql_select_db($db, $connection);
	$jokeResult = mysql_query($jokeQuery);	
	$jokeArray = array();
	
	while($itemRow = mysql_fetch_array($jokeResult))
	{
		$jokeArray[] = array( $mysql_id => $itemRow[$mysql_id],
							$mysql_joke => $itemRow[$mysql_joke],
							$mysql_author => $itemRow[$mysql_author],
							$mysql_statistic => $itemRow[$mysql_statistic],
							$mysql_statisticURL => $itemRow[$mysql_statisticURL],
							$mysql_dateAdded => $itemRow[$mysql_dateAdded],
							$mysql_charity => $itemRow[$mysql_charity],
							$mysql_charityURL => $itemRow[$mysql_charityURL],
							$mysql_favorite => $itemRow[$mysql_favorite],
							$mysql_categoryID => $itemRow[$mysql_categoryID]);
	}
	mysql_close($connection);
	
	// encode the results as JSON Text...
	$returnItems = $jokeArray;
							
	$JSONResult = json_encode($returnItems);
	
	// print the results so that our app can read them
	echo $JSONResult;
?>
