﻿CREATE LOGIN [SFActor] WITH PASSWORD = 'SFActor'
GO

CREATE USER [SFActor]
	FROM LOGIN [SFActor]
	WITH DEFAULT_SCHEMA = dbo

GO

ALTER ROLE db_owner ADD MEMBER SFActor