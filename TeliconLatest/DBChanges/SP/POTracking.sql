USE `teliconlatestdb`;
DROP procedure IF EXISTS `POTracking`;

DELIMITER $$
USE `teliconlatestdb`$$
CREATE PROCEDURE `POTracking` (in pPONum text,in pDateFrom date,in pDateTo date)
BEGIN
-- TODO below query will change
SELECT Workid, Wo_ref, Wo_title, Requestdt, Dispatchdt, Status, PONum, ClassName, Wo_client FROM TRN23100;
END$$

DELIMITER ;
