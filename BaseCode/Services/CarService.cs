using BaseCode.Models;
using BaseCode.Models.Tables;
using BaseCode.Models.Dealership.Requests;
using BaseCode.Models.Dealership.Responses;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace BaseCode.Services
{
    public class CarService
    {
        private readonly DealershipDBContext _dbContext;

        public CarService(DealershipDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public GetAllCarsResponse GetAllCars()
        {
            var response = new GetAllCarsResponse
            {
                Cars = new List<Cars>()
            };

            try
            {
                string query = @"
                    SELECT 
                        CAR_ID, CAR_MODEL, CAR_BRAND, CAR_HORSEPOWER,
                        CAR_SEATER, CAR_COLOR, CAR_PRICE, CAR_STATUS,
                        CAR_CREATE_DATE, CAR_UPDATE_DATE
                    FROM CAR
                    WHERE CAR_STATUS = 'A'";

                var dataTable = _dbContext.ExecuteQuery(query);
                
                foreach (DataRow row in dataTable.Rows)
                {
                    // Use the factory method instead of manual mapping
                    response.Cars.Add(Cars.FromDataRow(row));
                }

                response.IsSuccess = true;
                response.Message = "Cars retrieved successfully";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error retrieving cars: {ex.Message}";
            }

            return response;
        }

        public GetCarResponse GetCarById(GetCarByIdRequest request)
        {
            var response = new GetCarResponse();

            try
            {
                string query = @"
                    SELECT 
                        CAR_ID, CAR_MODEL, CAR_BRAND, CAR_HORSEPOWER,
                        CAR_SEATER, CAR_COLOR, CAR_PRICE, CAR_STATUS,
                        CAR_CREATE_DATE, CAR_UPDATE_DATE
                    FROM CAR
                    WHERE CAR_ID = @CarId";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@CarId", request.CarId)
                };

                var dataTable = _dbContext.ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    // Use the factory method to create Car from DataRow and add to a new list
                    response.Cars = new List<Cars> { Cars.FromDataRow(dataTable.Rows[0]) };

                    response.IsSuccess = true;
                    response.Message = "Car retrieved successfully";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Car not found";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error retrieving car: {ex.Message}";
            }

            return response;
        }

        public GetCarResponse GetCarByName(GetCarByNameRequest request)
        {
            var response = new GetCarResponse { Cars = new List<Cars>() };

            try
            {
                string query = @"
                    SELECT 
                        CAR_ID, CAR_MODEL, CAR_BRAND, CAR_HORSEPOWER,
                        CAR_SEATER, CAR_COLOR, CAR_PRICE, CAR_STATUS,
                        CAR_CREATE_DATE, CAR_UPDATE_DATE
                    FROM CAR
                    WHERE (CAR_MODEL LIKE @SearchTerm OR CAR_BRAND LIKE @SearchTerm)
                    AND CAR_STATUS = 'A'";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@SearchTerm", $"%{request.SearchTerm}%")
                };

                var dataTable = _dbContext.ExecuteQuery(query, parameters);

                foreach (DataRow row in dataTable.Rows)
                {
                    // Use the factory method to create Car from DataRow
                    response.Cars.Add(Cars.FromDataRow(row));
                }

                response.IsSuccess = true;
                response.Message = $"{response.Cars.Count} car(s) found matching '{request.SearchTerm}'";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error searching for cars: {ex.Message}";
            }

            return response;
        }
        
        public BasicResponse CreateCar(CreateCarRequest request)
        {
            var response = new BasicResponse();
            
            try
            {
                string query = @"
                    INSERT INTO CAR (
                        CAR_MODEL, CAR_BRAND, CAR_HORSEPOWER, CAR_SEATER,
                        CAR_COLOR, CAR_PRICE, CAR_STATUS
                    ) VALUES (
                        @CarModel, @CarBrand, @CarHorsepower, @CarSeater,
                        @CarColor, @CarPrice, 'A'
                    )";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@CarModel", request.CarModel),
                    new MySqlParameter("@CarBrand", request.CarBrand),
                    new MySqlParameter("@CarHorsepower", request.CarHorsepower),
                    new MySqlParameter("@CarSeater", request.CarSeater),
                    new MySqlParameter("@CarColor", request.CarColor),
                    new MySqlParameter("@CarPrice", request.CarPrice)
                };

                int rowsAffected = _dbContext.ExecuteNonQuery(query, parameters);
                
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.Message = "Car created successfully";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Failed to create car";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error creating car: {ex.Message}";
            }
            
            return response;
        }
        
        public BasicResponse UpdateCar(UpdateCarRequest request)
        {
            var response = new BasicResponse();
            
            try
            {
                string query = @"
                    UPDATE CAR SET
                        CAR_MODEL = @CarModel,
                        CAR_BRAND = @CarBrand,
                        CAR_HORSEPOWER = @CarHorsepower,
                        CAR_SEATER = @CarSeater,
                        CAR_COLOR = @CarColor,
                        CAR_PRICE = @CarPrice,
                        CAR_UPDATE_DATE = CURRENT_TIMESTAMP()
                    WHERE CAR_ID = @CarId";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@CarId", request.CarId),
                    new MySqlParameter("@CarModel", request.CarModel),
                    new MySqlParameter("@CarBrand", request.CarBrand),
                    new MySqlParameter("@CarHorsepower", request.CarHorsepower),
                    new MySqlParameter("@CarSeater", request.CarSeater),
                    new MySqlParameter("@CarColor", request.CarColor),
                    new MySqlParameter("@CarPrice", request.CarPrice)
                };

                int rowsAffected = _dbContext.ExecuteNonQuery(query, parameters);
                
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.Message = "Car updated successfully";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Car not found or no changes made";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error updating car: {ex.Message}";
            }
            
            return response;
        }
        
        public BasicResponse DeleteCar(int carId)
        {
            var response = new BasicResponse();
            
            try
            {
                // Soft delete by setting status to 'D'
                string query = @"
                    UPDATE CAR SET
                        CAR_STATUS = 'D',
                        CAR_UPDATE_DATE = CURRENT_TIMESTAMP()
                    WHERE CAR_ID = @CarId";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@CarId", carId)
                };

                int rowsAffected = _dbContext.ExecuteNonQuery(query, parameters);
                
                if (rowsAffected > 0)
                {
                    response.IsSuccess = true;
                    response.Message = "Car deleted successfully";
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Car not found";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error deleting car: {ex.Message}";
            }
            
            return response;
        }

        public PaginatedCarsResponse GetPaginatedCars(GetPaginatedCarsRequest request)
        {
            var response = new PaginatedCarsResponse
            {
                CurrentPage = request.PageNumber,
                PageSize = request.PageSize
            };

            try
            {
                // First, get the total count of active cars
                string countQuery = "SELECT COUNT(*) FROM CAR WHERE CAR_STATUS = 'A'";
                
                int totalRecords = Convert.ToInt32(_dbContext.ExecuteScalar(countQuery));
                response.TotalRecords = totalRecords;
                
                // Calculate total pages
                response.TotalPages = (int)Math.Ceiling(totalRecords / (double)request.PageSize);
                
                // Skip (PageNumber-1) * PageSize records and take PageSize records
                int skip = (request.PageNumber - 1) * request.PageSize;
                
                string query = @"
                    SELECT 
                        CAR_ID, CAR_MODEL, CAR_BRAND, CAR_HORSEPOWER,
                        CAR_SEATER, CAR_COLOR, CAR_PRICE, CAR_STATUS,
                        CAR_CREATE_DATE, CAR_UPDATE_DATE
                    FROM CAR
                    WHERE CAR_STATUS = 'A'
                    ORDER BY CAR_ID ASC
                    LIMIT @PageSize OFFSET @Skip";

                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@PageSize", request.PageSize),
                    new MySqlParameter("@Skip", skip)
                };

                var dataTable = _dbContext.ExecuteQuery(query, parameters);
                
                foreach (DataRow row in dataTable.Rows)
                {
                    response.Cars.Add(Cars.FromDataRow(row));
                }

                response.IsSuccess = true;
                response.Message = $"Retrieved page {request.PageNumber} of {response.TotalPages} (Total records: {totalRecords})";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error retrieving paginated cars: {ex.Message}";
            }

            return response;
        }
    }
}