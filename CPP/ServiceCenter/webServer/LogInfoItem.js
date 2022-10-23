class LogItemInfo
{
    /// <summary>
    /// 服务名
    /// </summary>
  ServiceName ;
/// <summary>
/// 触发的日期
/// </summary>
 FireDate;

/// <summary>
/// 触发时间（时分秒）
/// </summary>
  FireTime;

/// <summary>
/// 触发等级 DEBUG INFO  WARN ERROR
/// </summary>
  Level;

/// <summary>
/// Id 可选
/// </summary>
  Id;

/// <summary>
/// 日志内容
/// </summary>
  Content;

}

module.exports = LogItemInfo ;