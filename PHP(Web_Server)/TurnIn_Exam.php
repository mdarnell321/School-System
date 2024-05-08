<?php
include 'connection.php';
       
	$user_id = $_GET['uid'];
    $test_arr= $_GET['tarr'];
    $test_id= $_GET['tid'];
    $password = $_GET['pass'];
    {//security protocol
        $security_id = $user_id;
        $security_pass = $password;
        include 'Security.php';

    }
    date_default_timezone_set('America/Chicago');
    $curtime = date('Y-m-d H:i:s');


    function destroy_answers($arr, $test, $user)
    {
        for ($i = 0; $i < (count($arr) - 1); $i++) 
        {
             $query = "DELETE FROM test_turned_in_answers WHERE Question_ID = '$i' AND Test_ID = '$test' AND User_ID = '$user' ";
             mysqli_query($con, $query);
        }
    }

    $test_ans_details = explode('|',$test_arr);
    $correct_answers = 0;
    for ($i = 0; $i < (count($test_ans_details) - 1); $i++) 
    {
        {
            $query = "SELECT q.Answer FROM test_question AS q WHERE ID = '$i' AND Test_ID = '$test_id'";
            $result = mysqli_query($con, $query);
            $num_results = mysqli_num_rows($result);  
	
            if($num_results === 0)
            {
		        echo "Something went wrong.";
		        return;
            }
            else 
            {
                {
                    $response = $test_ans_details[$i];
                     $insert_data = "INSERT INTO test_turned_in_answers VALUES('$test_id', '$user_id', '$i', '$response')
                                     ON DUPLICATE KEY UPDATE 
                                     Response = VALUES(Response);
";
                     $data_check = mysqli_query($con, $insert_data);
                     if(!$data_check)
                     {
                         echo "Error in turning in test (B).";
                         destroy_answers($test_ans_details,$test_id,$user_id);
                         return;
                     }
                }
                $row = mysqli_fetch_assoc($result);
	            if((int)$row['Answer'] === (int)$test_ans_details[$i])
                {
                    $correct_answers ++;
                }
            }
        }
    }
    $tuple_exists = false;
    $calculated_grade = (int)(((float)$correct_answers / (count($test_ans_details) - 1)) * 100);

    {
            $query = "SELECT * FROM test_turned_in WHERE Test_ID = '$test_id' AND User_ID = '$user_id'";
            $result = mysqli_query($con, $query);

		    $tuple_exists = (mysqli_num_rows($result) !== 0);
    }

    if($tuple_exists === false)
    {
        $insert_data = "INSERT INTO test_turned_in VALUES('$test_id', '$user_id', '$curtime', '$calculated_grade')";
        $data_check = mysqli_query($con, $insert_data);
        if($data_check)
        {
            echo "/Success";
        }
        else 
        {      
            echo "Error in turning in test.";
            destroy_answers($test_ans_details,$test_id,$user_id);
        }
    }
    else 
    {
        $query = 
    
        "UPDATE test_turned_in
         SET Grade = '$calculated_grade'
         WHERE User_ID = '$user_id' AND Test_ID = '$test_id'
        ";

        $result = mysqli_query($con, $query);
        if($result)
		    echo "/Success.";
        else
        {
            destroy_answers($test_ans_details,$test_id,$user_id);
            echo "Error.";
        }
          
    }

    
?>
