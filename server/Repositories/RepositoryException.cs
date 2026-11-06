namespace SmartCv.Repositories;

public class RepositoryException(string message, Exception innerException) : Exception(message, innerException);
