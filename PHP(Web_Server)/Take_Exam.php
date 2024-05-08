<?php
include 'connection.php';
       
    $module_id =  $_GET['mid'];
    $user_id =  $_GET['uid'];
    $password = $_GET['pass'];
    {//security protocol
        $security_id = $user_id;
        $security_pass = $password;
        include 'Security.php';

    }


    $query =     "SELECT *
                  FROM test
                  WHERE Module_ID = '$module_id'
                  ";
    $result = mysqli_query($con, $query);
    $num_results = mysqli_num_rows($result);  
	
    if($num_results === 0)
    {
        echo "Module does not contain a test.";
		return;
    }
    $row = mysqli_fetch_assoc($result);
    $test_id = $row['ID'];

    {//check if test turnedin
        $query2 =    "SELECT *
                      FROM test_turned_in
                      WHERE Test_ID = '$test_id' AND User_ID = '$user_id'
                      ";
        $result2 = mysqli_query($con, $query2);
        $num_results2 = mysqli_num_rows($result2);  
        if($num_results2 > 0)
        {
            echo "You have already taken this exam.";
		    return;
        }
    }
    
   
    $query2 =    "SELECT *
                  FROM test_question
                  WHERE Test_ID = '$test_id'
                  ";
    $result2 = mysqli_query($con, $query2);
    $num_results2 = mysqli_num_rows($result2);
    if($num_results2 === 0)
    {
        echo "Error retrieving questions.";
		return;
    }
    $test_data = "";
    while($row2 = mysqli_fetch_assoc($result2)) {
        $options = "";
        $question_id = $row2['ID'];
         {//fetch options

            $query3 =    "SELECT o._Option
                  FROM test_question_options AS o
                  WHERE Test_ID = '$test_id' AND Question_ID = '$question_id'
                  ";
            $result3 = mysqli_query($con, $query3);
            $num_results3 = mysqli_num_rows($result3);  
            if($num_results3 === 0)
            {
                echo "Error retrieving questions options.";
		        return;
            }
            while($row3 = mysqli_fetch_assoc($result3)) {
                 $options .=  $row3['_Option'] . "\n";
            }
        }
        $test_data .= $question_id . '~' . $row2['Question'] . '~' . $options . '~' . $row2['Answer'] . '`';
    }

   
    echo  '/' . $row['ID'] . '|' . $row['Due'] . '|' . $row['Time_Limit'] . '|' . $test_data;
    

?>
