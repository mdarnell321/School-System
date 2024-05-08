<?php
include 'connection.php';
    
	$teacher_status =  $_GET['teach'];

	$query = "
	SELECT DISTINCT * 
	FROM class AS c, user AS u
	WHERE c.Professor_id = u.ID;
	";
	if($teacher_status == 1)
	{
		$query = "
					SELECT DISTINCT * 
					FROM class AS c
					WHERE c.Professor_id IS NULL;
					";
	}
    $result = mysqli_query($con, $query);
    $count = mysqli_num_rows($result);  
	$return_val = "/";
    if($count > 0)
    {
		while($row = mysqli_fetch_assoc($result)) {

			$return_val .=  $row['Name'] . '|' . ($teacher_status == 0 ? ($row['First'] . ' ' . $row['Last']) : "Unassigned") . '|' . $row['Description'] . '|' . $row['Course_Branch'] . '|' . $row['Course_Number'] . '\\';
		}
    }
	echo $return_val;
?>
