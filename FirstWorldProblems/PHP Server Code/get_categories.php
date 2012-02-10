<?
	include 'mysql_vars.php';
	
	// Construct our MySQL query
	
	$categoryQuery = "SELECT * FROM `$mysql_categoryTable` $itemQueryAddition ORDER BY `$mysql_dateAdded` DESC";

	// execute the query and gather the results...
	mysql_select_db($db, $connection);
	$categoryResult = mysql_query($categoryQuery);	
	$categoryArray = array();
	
	while($itemRow = mysql_fetch_array($categoryResult))
	{
		$categoryArray[] = array( $mysql_categoryID => $itemRow[$mysql_categoryID],
								  $mysql_categoryText => $itemRow[$mysql_categoryText],
								  $mysql_viewCategoryFilter => $itemRow[$mysql_viewCategoryFilter],
								  $mysql_dateAdded => $itemRow[$mysql_dateAdded]);
	}
	mysql_close($connection);
	
	// encode the results as JSON Text...
	$returnItems = $categoryArray;
							
	$JSONResult = json_encode($returnItems);
	
	// print the results so that our app can read them
	echo $JSONResult;
?>
