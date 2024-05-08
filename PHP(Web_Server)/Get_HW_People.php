<?php
include 'connection.php';
       
    $module_id =  $_GET['mid'];
    
    
    $query =    "SELECT h.User_ID, a.ID, h.Work, h.Grade, u.First, u.Last
                  FROM hw_turned_in AS h, assignment AS a, user AS u
                  WHERE a.Module_ID = '$module_id' AND a.ID = h.Assignment_ID AND h.User_ID = u.ID
                  ";
    $result = mysqli_query($con, $query);
    $num_results = mysqli_num_rows($result);  

    $data = "/";
    while($row = mysqli_fetch_assoc($result)) {
        $data .= $row['First'] . ' ' . $row['Last'] . '~' . $row['User_ID'] . '~' . $row['ID'] . '~' . $row['Work']  . '~' . $row['Grade'] . ($row['Grade'] != ""? '%' : "") .'|';
    }
    echo $data;
    

?>
